namespace PaymentGateway.Api.Services.AcquiringBank;

/// <summary>
/// Wire shape received from the bank simulator. Authorized is nullable so a
/// 200 without an explicit decision is distinguishable from a real decline;
/// </summary>
public sealed record BankPaymentResponse(bool? Authorized, string? AuthorizationCode);
