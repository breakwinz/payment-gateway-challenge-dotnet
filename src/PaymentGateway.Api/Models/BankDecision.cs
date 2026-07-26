namespace PaymentGateway.Api.Models;

/// <summary>
/// Outcome of a single acquiring-bank call. Distinct from PaymentStatus:
/// the bank never sees invalid requests (no Rejected), and Unavailable means
/// no payment was created at all (gateway 502).
/// </summary>
public enum BankDecision
{
    Authorized,
    Declined,
    Unavailable
}
