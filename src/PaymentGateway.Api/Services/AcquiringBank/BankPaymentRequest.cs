namespace PaymentGateway.Api.Services.AcquiringBank;

/// <summary>
/// Wire shape sent to the bank simulator; serialized snake_case, expiry
/// pre-formatted as "MM/yyyy".
/// </summary>
public sealed record BankPaymentRequest(string CardNumber, string ExpiryDate, string Currency, int Amount, string Cvv);
