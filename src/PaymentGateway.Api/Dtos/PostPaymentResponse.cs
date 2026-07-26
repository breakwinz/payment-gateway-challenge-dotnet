using PaymentGateway.Api.Models;

namespace PaymentGateway.Api.Dtos;

/// <summary>
/// 201 body for a processed (Authorized or Declined) payment.
/// Card number reduced to its last four digits.
/// </summary>
public class PostPaymentResponse
{
    public Guid Id { get; init; }
    public PaymentStatus Status { get; init; }

    // This was an Int in the example provided, but leading zeros would disappear.
    // Changed to string, this aligns the type with the Post-body full PAN as well.
    public string CardNumberLastFour { get; init; } = string.Empty;
    public int ExpiryMonth { get; init; }
    public int ExpiryYear { get; init; }
    public string Currency { get; init; } = string.Empty;
    public int Amount { get; init; }
}
