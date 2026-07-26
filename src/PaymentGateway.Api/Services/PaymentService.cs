using PaymentGateway.Api.Dtos;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Services.AcquiringBank;

namespace PaymentGateway.Api.Services;

/// <summary>
/// Orchestrates a payment: asks the bank for a decision, stores the outcome,
/// serves retrieval. Only the last four card digits survive past the bank call.
/// </summary>
public class PaymentService(
    IBankClient bankClient,
    IPaymentsRepository repository,
    ILogger<PaymentService> logger)
{
    public async Task<PaymentProcessResult> ProcessAsync(PostPaymentRequest request)
    {
        var decision = await bankClient.AuthorizeAsync(request);
        if (decision == BankDecision.Unavailable)
        {
            logger.LogError("Bank outcome unavailable; no payment stored for card ending {CardNumberLastFour}",
                request.CardNumber[^4..]);
            return PaymentProcessResult.Unavailable();
        }

        var payment = new Payment(
            Guid.NewGuid(),
            decision == BankDecision.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined,
            request.CardNumber[^4..],
            request.ExpiryMonth,
            request.ExpiryYear,
            request.Currency,
            request.Amount);
        await repository.AddAsync(payment);
        logger.LogInformation("Payment {PaymentId} {Status}: {Currency} {Amount} card ending {CardNumberLastFour}",
            payment.Id, payment.Status, payment.Currency, payment.Amount, payment.CardNumberLastFour);
        return PaymentProcessResult.Processed(payment);
    }

    public Task<Payment?> GetPaymentAsync(Guid id) => repository.GetAsync(id);
}
