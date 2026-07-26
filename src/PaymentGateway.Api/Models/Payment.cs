namespace PaymentGateway.Api.Models;

/// <summary>
/// Stored payment record. Immutable, repository consumers cannot
/// mutate stored state through a returned reference. Deliberately not the
/// API response DTO, so a persistent store never couples to the HTTP shape.
/// </summary>
public sealed record Payment(
    Guid Id,
    PaymentStatus Status,
    string CardNumberLastFour,
    int ExpiryMonth,
    int ExpiryYear,
    string Currency,
    int Amount);
