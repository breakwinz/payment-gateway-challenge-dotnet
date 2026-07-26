namespace PaymentGateway.Api.Models;

/// <summary>
/// Result category of processing a payment:
/// Processed (stored, 201 with Authorized/Declined) or BankUnavailable (nothing stored, 502).
/// </summary>
public enum PaymentOutcome
{
    Processed,
    BankUnavailable
}
