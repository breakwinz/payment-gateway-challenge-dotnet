using PaymentGateway.Api.Dtos;

namespace PaymentGateway.Api.Tests.TestSupport;

public static class TestPaymentRequest
{
    // The supplied simulator authorizes cards ending in an odd digit.
    public static PostPaymentRequest Valid(
        string cardNumber = "22224053432488771",
        int amount = 100,
        string currency = "GBP",
        int? expiryYear = null,
        string cvv = "123") => new()
    {
        CardNumber = cardNumber,
        ExpiryMonth = 4,
        ExpiryYear = expiryYear ?? DateTime.UtcNow.Year + 1,
        Currency = currency,
        Amount = amount,
        Cvv = cvv
    };
}
