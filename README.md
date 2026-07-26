# Payment Gateway — .NET Challenge Submission

An ASP.NET Core API that validates card payments, sends them to the supplied
acquiring-bank simulator, stores successful bank decisions, and retrieves
payments by id.

## Run

```bash
docker compose up -d
dotnet test
dotnet run --project src/PaymentGateway.Api
```

To run only the tests that do not need Docker:

```bash
dotnet test --filter "Category!=Integration"
```

Swagger UI is available at `/swagger` in Development. The bank address and
timeout are configured in `appsettings.json`:

```json
"BankSimulator": {
  "BaseUrl": "http://localhost:8080",
  "TimeoutMilliseconds": 5000
}
```

## API

### POST /api/payments

```json
{
  "cardNumber": "22224053430043",
  "expiryMonth": 4,
  "expiryYear": 2027,
  "currency": "GBP",
  "amount": 1050,
  "cvv": "123"
}
```

An authorized payment returns `201 Created`, a `Location` header, and:

```json
{
  "id": "ed267f80-32bf-4ba8-96cb-c13c34df32b2",
  "status": "Authorized",
  "cardNumberLastFour": "0043",
  "expiryMonth": 4,
  "expiryYear": 2027,
  "currency": "GBP",
  "amount": 1050
}
```

| Outcome | HTTP response | Stored |
|---|---|---|
| Bank authorized | `201` with an `Authorized` payment | Yes |
| Bank declined | `201` with a `Declined` payment | Yes |
| Invalid merchant request | `400 Rejected` with validation errors | No |
| Bank failure or unknown bank outcome | `502 ProblemDetails` | No |

Validation rules:

- Card number: 14–19 numeric characters.
- Expiry: month 1–12 and not before the current month.
- Currency: `GBP`, `USD`, or `EUR`.
- Amount: positive integer in minor currency units.
- CVV: 3–4 numeric characters.

### GET /api/payments/{id}

Returns the stored payment with `200 OK`, or `404 Not Found`. The full card
number and CVV are never returned.

## Design

- The controller owns HTTP mapping; `PaymentService` coordinates the bank and
  repository.
- `BankClient` is a typed `HttpClient` and uses the simulator's snake_case wire
  format and `MM/yyyy` expiry.
- `InMemoryPaymentsRepository` uses a `ConcurrentDictionary`, as durable
  storage is outside the challenge scope.
- Stored payments are immutable and contain only the last four card digits.
  Full PAN and CVV are not stored, returned, or logged.
- `Authorized` and `Declined` decisions are stored. Invalid requests are
  rejected before the bank is called.
- A non-success response, timeout, connection failure, or response without an
  explicit bank decision returns `502` and creates no payment. The failure is
  logged using only the card's last four digits.

A timeout or lost response does not prove that the bank failed to authorize the
payment. In that case the outcome is unknown: the gateway has no payment record,
but the bank may have processed it. Retrying is unsafe without idempotency or
reconciliation. The request-abort token is therefore not forwarded to the bank;
`HttpClient.Timeout` bounds how long the gateway waits, but does not resolve the
unknown-outcome problem.

## Testing

The suite uses the smallest useful seam for each behaviour:

- `PaymentGatewayIntegrationTests` sends real HTTP through the gateway to the
  supplied Docker bank. It covers authorized, declined, rejected, bank
  unavailable, retrieval, and not found.
- `BankClientTests` uses one in-memory `HttpMessageHandler` to verify the exact
  outgoing wire contract and response/error mapping without sockets or delays.
- `PostPaymentRequestValidationTests` covers valid inputs and validation
  boundaries with data-driven tests.
- `PaymentServiceTests` verifies that an unavailable or unknown bank result is
  not stored, while Authorized and Declined decisions are mapped and stored.
- `InMemoryPaymentsRepositoryTests` covers round-trip storage and an unknown id.

No mocking library, custom HTTP server, or additional dependency is used.

## Assumptions

- The challenge does not prescribe HTTP status codes; `201`, `400`, `404`, and
  `502` are this submission's choices.
- Cards remain valid through the end of their expiry month.
- Currency matching is case-sensitive.
- The simulator's `authorization_code` is not required by the gateway contract,
  so it is not stored or returned.

## Production follow-ups

- Add merchant-scoped idempotency keys and bank reconciliation for ambiguous
  outcomes.
- Replace the in-memory repository with durable encrypted storage.
- Publish a redacted operational event to a durable stream such as Kafka when a
  bank call fails or its outcome is unknown. A consumer can alert business and
  technical teams and start reconciliation. With durable storage, use an outbox
  so the payment state and event cannot drift.
- Add correlation ids, distributed tracing, metrics, and bank health alerts.
- Add backpressure and a circuit breaker. Do not add automatic payment retries
  until idempotency is guaranteed.

## Dependencies added beyond the template

None.
