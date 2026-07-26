using System.ComponentModel.DataAnnotations;
using PaymentGateway.Api.Dtos;
using PaymentGateway.Api.Tests.TestSupport;

namespace PaymentGateway.Api.Tests.Unit;

public class PostPaymentRequestValidationTests
{
    [Fact]
    public void ValidRequest_IsAccepted() => Assert.True(IsValid(TestPaymentRequest.Valid()));

    [Theory]
    [InlineData("", false)]
    [InlineData("1234567890123", false)]
    [InlineData("12345678901234", true)]
    [InlineData("1234567890123456789", true)]
    [InlineData("12345678901234567890", false)]
    [InlineData("1234567890123a", false)]
    public void CardNumber_IsValidated(string cardNumber, bool expected)
    {
        var request = TestPaymentRequest.Valid();
        request.CardNumber = cardNumber;

        Assert.Equal(expected, IsValid(request));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(12, true)]
    [InlineData(13, false)]
    public void ExpiryMonth_IsValidated(int month, bool expected)
    {
        var request = TestPaymentRequest.Valid();
        request.ExpiryMonth = month;

        Assert.Equal(expected, IsValid(request));
    }

    [Fact]
    public void ExpiryThisMonth_IsAccepted()
    {
        var today = DateTime.UtcNow;
        var request = TestPaymentRequest.Valid();
        request.ExpiryMonth = today.Month;
        request.ExpiryYear = today.Year;

        Assert.True(IsValid(request));
    }

    [Fact]
    public void ExpiryLastMonth_IsRejected()
    {
        var lastMonth = DateTime.UtcNow.AddMonths(-1);
        var request = TestPaymentRequest.Valid();
        request.ExpiryMonth = lastMonth.Month;
        request.ExpiryYear = lastMonth.Year;

        Assert.False(IsValid(request));
    }

    [Fact]
    public void MissingExpiryYear_IsRejected()
    {
        var request = TestPaymentRequest.Valid();
        request.ExpiryYear = 0;

        Assert.False(IsValid(request));
    }

    [Theory]
    [InlineData("GBP", true)]
    [InlineData("USD", true)]
    [InlineData("EUR", true)]
    [InlineData("JPY", false)]
    [InlineData("gbp", false)]
    [InlineData("", false)]
    public void Currency_IsValidated(string currency, bool expected)
    {
        var request = TestPaymentRequest.Valid();
        request.Currency = currency;

        Assert.Equal(expected, IsValid(request));
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, false)]
    [InlineData(1, true)]
    public void Amount_IsValidated(int amount, bool expected)
    {
        var request = TestPaymentRequest.Valid();
        request.Amount = amount;

        Assert.Equal(expected, IsValid(request));
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("12", false)]
    [InlineData("123", true)]
    [InlineData("1234", true)]
    [InlineData("12345", false)]
    [InlineData("12a", false)]
    public void Cvv_IsValidated(string cvv, bool expected)
    {
        var request = TestPaymentRequest.Valid();
        request.Cvv = cvv;

        Assert.Equal(expected, IsValid(request));
    }

    private static bool IsValid(PostPaymentRequest request) =>
        Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            validationResults: null,
            validateAllProperties: true);
}
