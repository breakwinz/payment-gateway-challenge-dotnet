using PaymentGateway.Api.Models;

namespace PaymentGateway.Api.Services;

/// <summary>
/// Storage of processed payments. Callers depend only on this interface, so a
/// persistent implementation can replace the in-memory one without touching them.
/// </summary>
public interface IPaymentsRepository
{
    Task AddAsync(Payment payment);
    Task<Payment?> GetAsync(Guid id);
}
