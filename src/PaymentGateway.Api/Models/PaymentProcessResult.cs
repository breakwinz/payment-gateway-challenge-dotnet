namespace PaymentGateway.Api.Models;

/// <summary>
/// What PaymentService returns to the controller: the outcome plus the stored
/// payment when one was created (null when the bank was unavailable).
/// </summary>
public record PaymentProcessResult(PaymentOutcome Outcome, Payment? Payment)
{
    public static PaymentProcessResult Processed(Payment payment) =>
        new(PaymentOutcome.Processed, payment);

    public static PaymentProcessResult Unavailable() =>
        new(PaymentOutcome.BankUnavailable, null);
}
