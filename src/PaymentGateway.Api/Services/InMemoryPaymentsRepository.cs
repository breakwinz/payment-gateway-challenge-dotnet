using System.Collections.Concurrent;
using PaymentGateway.Api.Models;

namespace PaymentGateway.Api.Services;

/// <summary>
/// Thread-safe in-memory store — the only implementation the challenge needs
/// (a test double repository is explicitly permitted).
/// </summary>
public class InMemoryPaymentsRepository : IPaymentsRepository
{
    private readonly ConcurrentDictionary<Guid, Payment> _payments = new();

    public Task AddAsync(Payment payment)
    {
        _payments[payment.Id] = payment;
        return Task.CompletedTask;
    }

    public Task<Payment?> GetAsync(Guid id) =>
        Task.FromResult(_payments.TryGetValue(id, out var payment) ? payment : null);
}
