using System.Net;
using System.Text;
using System.Text.Json;
using PaymentGateway.Api.Dtos;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Services.AcquiringBank;

namespace PaymentGateway.Api.Tests.Unit;

public class BankClientTests
{
    private static readonly PostPaymentRequest Request = new()
    {
        CardNumber = "22224053432488771",
        ExpiryMonth = 4,
        ExpiryYear = 2027,
        Currency = "GBP",
        Amount = 1050,
        Cvv = "0456"
    };

    [Fact]
    public async Task Authorize_SendsTheBankWireFormat()
    {
        string? requestBody = null;
        var client = CreateClient(async (request, cancellationToken) =>
        {
            requestBody = await request.Content!.ReadAsStringAsync(cancellationToken);
            return Json("""{"authorized":true}""");
        });

        await client.AuthorizeAsync(Request);

        using var json = JsonDocument.Parse(requestBody!);
        var root = json.RootElement;
        Assert.Equal(Request.CardNumber, root.GetProperty("card_number").GetString());
        Assert.Equal("04/2027", root.GetProperty("expiry_date").GetString());
        Assert.Equal(Request.Currency, root.GetProperty("currency").GetString());
        Assert.Equal(Request.Amount, root.GetProperty("amount").GetInt32());
        Assert.Equal(Request.Cvv, root.GetProperty("cvv").GetString());
    }

    [Theory]
    [InlineData(true, BankDecision.Authorized)]
    [InlineData(false, BankDecision.Declined)]
    public async Task Authorize_MapsTheBankDecision(bool authorized, BankDecision expected)
    {
        var client = CreateClient((_, _) =>
            Task.FromResult(Json($$"""{"authorized":{{authorized.ToString().ToLowerInvariant()}}}""")));

        var decision = await client.AuthorizeAsync(Request);

        Assert.Equal(expected, decision);
    }

    [Fact]
    public async Task Authorize_NonSuccessResponse_IsUnavailable()
    {
        var client = CreateClient((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)));

        var decision = await client.AuthorizeAsync(Request);

        Assert.Equal(BankDecision.Unavailable, decision);
    }

    [Fact]
    public async Task Authorize_ResponseWithoutDecision_IsUnavailable()
    {
        var client = CreateClient((_, _) => Task.FromResult(Json("{}")));

        var decision = await client.AuthorizeAsync(Request);

        Assert.Equal(BankDecision.Unavailable, decision);
    }

    [Fact]
    public async Task Authorize_TransportFailure_IsUnavailable()
    {
        var client = CreateClient((_, _) =>
            Task.FromException<HttpResponseMessage>(new HttpRequestException("Connection failed")));

        var decision = await client.AuthorizeAsync(Request);

        Assert.Equal(BankDecision.Unavailable, decision);
    }

    private static BankClient CreateClient(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) =>
        new(new HttpClient(new StubHandler(send))
        {
            BaseAddress = new Uri("http://bank.test"),
            Timeout = TimeSpan.FromSeconds(1)
        });

    private static HttpResponseMessage Json(string body) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };

    private sealed class StubHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => send(request, cancellationToken);
    }
}
