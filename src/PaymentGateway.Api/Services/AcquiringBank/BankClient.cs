using System.Text.Json;
using PaymentGateway.Api.Dtos;
using PaymentGateway.Api.Models;

namespace PaymentGateway.Api.Services.AcquiringBank;

/// <summary>
/// Typed HttpClient for the bank simulator's wire contract (snake_case JSON, expiry as "MM/yyyy").
/// Anything short of an explicit yes/no, I.E non-2xx, timeout, connection failure, malformed body, all map to Unavailable.
/// </summary>
public class BankClient(HttpClient httpClient) : IBankClient
{
    private static readonly JsonSerializerOptions SnakeCase = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public async Task<BankDecision> AuthorizeAsync(PostPaymentRequest request)
    {
        var bankRequest = new BankPaymentRequest(
            request.CardNumber,
            $"{request.ExpiryMonth:D2}/{request.ExpiryYear:D4}",
            request.Currency,
            request.Amount,
            request.Cvv);

        try
        {
            using var response = await httpClient.PostAsJsonAsync("/payments", bankRequest, SnakeCase);
            if (!response.IsSuccessStatusCode)
            {
                return BankDecision.Unavailable;
            }

            var body = await response.Content.ReadFromJsonAsync<BankPaymentResponse>(SnakeCase);
            return body?.Authorized switch
            {
                true => BankDecision.Authorized,
                false => BankDecision.Declined,
                // 200 without an explicit decision (empty/wrong-shape body): the bank never said "yes/no".
                // Treat as service down rather than recording a false decline.
                null => BankDecision.Unavailable
            };
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException)
        {
            // The gateway cannot determine the bank outcome from a failed call or response.
            return BankDecision.Unavailable;
        }
    }
}
