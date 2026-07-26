using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Api.Dtos;

/// <summary>
/// Merchant's payment request. Carries full PAN and CVV, so instances
/// must never be stored or logged; validation failures become the 400 Rejected
/// response before the bank is called.
/// </summary>
public class PostPaymentRequest : IValidatableObject
{
    private static readonly string[] SupportedCurrencies = ["GBP", "USD", "EUR"];

    [Required(ErrorMessage = "Card number is required.")]
    [RegularExpression("^[0-9]{14,19}$", ErrorMessage = "Card number must be 14-19 numeric characters.")]
    public string CardNumber { get; set; } = string.Empty;

    [Range(1, 12, ErrorMessage = "Expiry month must be between 1 and 12.")]
    public int ExpiryMonth { get; set; }

    public int ExpiryYear { get; set; }

    [Required(ErrorMessage = "Currency is required.")]
    public string Currency { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Amount must be a positive integer in minor currency units.")]
    public int Amount { get; set; }

    [Required(ErrorMessage = "CVV is required.")]
    [RegularExpression("^[0-9]{3,4}$", ErrorMessage = "CVV must be 3-4 numeric characters.")]
    public string Cvv { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrEmpty(Currency) && !SupportedCurrencies.Contains(Currency))
        {
            yield return new ValidationResult(
                "Currency must be one of: GBP, USD, EUR.", [nameof(Currency)]);
        }

        if (ExpiryMonth is < 1 or > 12)
        {
            yield break; // month error already reported by [Range]
        }

        if (ExpiryYear > 9999)
        {
            yield return new ValidationResult(
                "Expiry year must be a four-digit year.", [nameof(ExpiryYear)]);
            yield break;
        }

        // Card is valid through the last day of its expiry month.
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (ExpiryYear < today.Year || (ExpiryYear == today.Year && ExpiryMonth < today.Month))
        {
            yield return new ValidationResult(
                "Card expiry (month/year) must be in the future.",
                [nameof(ExpiryMonth), nameof(ExpiryYear)]);
        }
    }
}
