using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Dtos;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Tests.TestSupport;

namespace PaymentGateway.Api.Tests.Integration;

/// <summary>
/// Requirement-level tests against the supplied bank simulator.
/// Run <c>docker-compose up -d</c> before this suite.
/// </summary>
[Trait("Category", "Integration")]
public class PaymentGatewayIntegrationTests : IDisposable
{
    private readonly WebApplicationFactory<PaymentsController> _factory =
        new WebApplicationFactory<PaymentsController>().WithWebHostBuilder(
            builder => builder.ConfigureLogging(logging => logging.ClearProviders()));

    [Fact]
    public async Task AuthorizedPayment_IsCreatedAndRetrievable()
    {
        using var client = _factory.CreateClient();
        var request = TestPaymentRequest.Valid("22224053430043");

        var response = await client.PostAsJsonAsync("/api/payments", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var payment = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();
        Assert.NotNull(payment);
        Assert.NotEqual(Guid.Empty, payment!.Id);
        Assert.Equal($"/api/payments/{payment.Id}", response.Headers.Location!.ToString());
        Assert.Equal(PaymentStatus.Authorized, payment.Status);
        Assert.Equal("0043", payment.CardNumberLastFour);
        Assert.Equal(request.ExpiryMonth, payment.ExpiryMonth);
        Assert.Equal(request.ExpiryYear, payment.ExpiryYear);
        Assert.Equal(request.Currency, payment.Currency);
        Assert.Equal(request.Amount, payment.Amount);

        var getResponse = await client.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var fetched = await getResponse.Content.ReadFromJsonAsync<GetPaymentResponse>();
        Assert.NotNull(fetched);
        Assert.Equal(payment.Id, fetched!.Id);
        Assert.Equal(payment.Status, fetched.Status);
        Assert.Equal(payment.CardNumberLastFour, fetched.CardNumberLastFour);
        Assert.Equal(payment.ExpiryMonth, fetched.ExpiryMonth);
        Assert.Equal(payment.ExpiryYear, fetched.ExpiryYear);
        Assert.Equal(payment.Currency, fetched.Currency);
        Assert.Equal(payment.Amount, fetched.Amount);
    }

    [Fact]
    public async Task DeclinedPayment_IsCreatedAndRetrievable()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/payments", TestPaymentRequest.Valid("22224053432488772"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var payment = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();
        Assert.Equal(PaymentStatus.Declined, payment!.Status);
        var fetched = await client.GetFromJsonAsync<GetPaymentResponse>(response.Headers.Location);
        Assert.Equal(PaymentStatus.Declined, fetched!.Status);
    }

    [Fact]
    public async Task InvalidPayment_IsRejected()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/payments", TestPaymentRequest.Valid(cardNumber: "1234"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var rejected = await response.Content.ReadFromJsonAsync<PostPaymentRejectedResponse>();
        Assert.Equal(PaymentStatus.Rejected, rejected!.Status);
        Assert.Contains("Card number must be 14-19 numeric characters.", rejected.Errors);
    }

    [Fact]
    public async Task BankUnavailable_ReturnsBadGateway()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/payments", TestPaymentRequest.Valid("22224053432488770"));

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal((int)HttpStatusCode.BadGateway, problem!.Status);
    }

    [Fact]
    public async Task UnknownPayment_ReturnsNotFound()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/payments/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    public void Dispose() => _factory.Dispose();
}
