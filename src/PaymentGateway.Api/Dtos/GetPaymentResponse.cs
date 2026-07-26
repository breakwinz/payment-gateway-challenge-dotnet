using PaymentGateway.Api.Models;

namespace PaymentGateway.Api.Dtos;

/// <summary>
/// 200 body for retrieving a previously made payment; same shape as the POST response.
/// </summary>
public class GetPaymentResponse
{
    public Guid Id { get; init; }
    public PaymentStatus Status { get; init; }
    public string CardNumberLastFour { get; init; } = string.Empty;
    public int ExpiryMonth { get; init; }
    public int ExpiryYear { get; init; }
    public string Currency { get; init; } = string.Empty;
    public int Amount { get; init; }
}
