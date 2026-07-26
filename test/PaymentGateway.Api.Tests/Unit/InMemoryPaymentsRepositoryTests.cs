using PaymentGateway.Api.Models;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests.Unit;

public class InMemoryPaymentsRepositoryTests
{
    [Fact]
    public async Task AddThenGet_ReturnsThePayment()
    {
        var repository = new InMemoryPaymentsRepository();
        var payment = Payment();

        await repository.AddAsync(payment);

        Assert.Equal(payment, await repository.GetAsync(payment.Id));
    }

    [Fact]
    public async Task GetUnknownPayment_ReturnsNull()
    {
        var repository = new InMemoryPaymentsRepository();

        Assert.Null(await repository.GetAsync(Guid.NewGuid()));
    }

    private static Payment Payment() => new(
        Guid.NewGuid(),
        PaymentStatus.Authorized,
        "0043",
        4,
        2027,
        "GBP",
        1050);
}
