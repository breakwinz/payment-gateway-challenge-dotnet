using Microsoft.Extensions.Logging.Abstractions;
using PaymentGateway.Api.Dtos;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Services.AcquiringBank;
using PaymentGateway.Api.Tests.TestSupport;

namespace PaymentGateway.Api.Tests.Unit;

public class PaymentServiceTests
{
    [Fact]
    public async Task BankUnavailable_DoesNotStoreAPayment()
    {
        var repository = new RecordingRepository();
        var service = new PaymentService(
            new StubBank(BankDecision.Unavailable),
            repository,
            NullLogger<PaymentService>.Instance);

        var payment = await service.ProcessAsync(TestPaymentRequest.Valid());

        Assert.Null(payment);
        Assert.Null(repository.AddedPayment);
    }

    [Theory]
    [InlineData(BankDecision.Authorized, PaymentStatus.Authorized)]
    [InlineData(BankDecision.Declined, PaymentStatus.Declined)]
    public async Task BankDecision_StoresTheMappedPayment(
        BankDecision decision,
        PaymentStatus expectedStatus)
    {
        var repository = new RecordingRepository();
        var service = new PaymentService(
            new StubBank(decision),
            repository,
            NullLogger<PaymentService>.Instance);
        var request = TestPaymentRequest.Valid("22224053430043", amount: 1050, currency: "USD");

        var payment = await service.ProcessAsync(request);

        Assert.NotNull(payment);
        Assert.Equal(expectedStatus, payment!.Status);
        Assert.Equal("0043", payment.CardNumberLastFour);
        Assert.Equal(request.ExpiryMonth, payment.ExpiryMonth);
        Assert.Equal(request.ExpiryYear, payment.ExpiryYear);
        Assert.Equal(request.Currency, payment.Currency);
        Assert.Equal(request.Amount, payment.Amount);
        Assert.Same(payment, repository.AddedPayment);
    }

    private sealed class StubBank(BankDecision decision) : IBankClient
    {
        public Task<BankDecision> AuthorizeAsync(PostPaymentRequest request) =>
            Task.FromResult(decision);
    }

    private sealed class RecordingRepository : IPaymentsRepository
    {
        public Payment? AddedPayment { get; private set; }

        public Task AddAsync(Payment payment)
        {
            AddedPayment = payment;
            return Task.CompletedTask;
        }

        public Task<Payment?> GetAsync(Guid id) => Task.FromResult<Payment?>(null);
    }
}
