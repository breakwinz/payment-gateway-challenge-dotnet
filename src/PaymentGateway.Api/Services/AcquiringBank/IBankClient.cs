using PaymentGateway.Api.Dtos;
using PaymentGateway.Api.Models;

namespace PaymentGateway.Api.Services.AcquiringBank;

/// <summary>
/// Requests an authorization decision from the acquiring bank.
/// </summary>
public interface IBankClient
{
    Task<BankDecision> AuthorizeAsync(PostPaymentRequest request);
}
