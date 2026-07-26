using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Api.Dtos;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers;

/// <summary>
/// HTTP surface of the gateway: POST /api/payments processes a card payment,
/// GET /api/payments/{id} retrieves a previously made one.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class PaymentsController : Controller
{
    private readonly PaymentService _paymentService;

    public PaymentsController(PaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost]
    public async Task<ActionResult<PostPaymentResponse>> PostPaymentAsync(PostPaymentRequest request)
    {
        var payment = await _paymentService.ProcessAsync(request);
        return payment is null
            ? Problem(
                title: "The payment outcome could not be determined. Contact Support.",
                statusCode: StatusCodes.Status502BadGateway)
            : Created($"/api/payments/{payment.Id}", ToResponse(payment));

        // local functions
        PostPaymentResponse ToResponse(Payment payment) => new()
        {
            Id = payment.Id,
            Status = payment.Status,
            CardNumberLastFour = payment.CardNumberLastFour,
            ExpiryMonth = payment.ExpiryMonth,
            ExpiryYear = payment.ExpiryYear,
            Currency = payment.Currency,
            Amount = payment.Amount
        };
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetPaymentResponse>> GetPaymentAsync(Guid id)
    {
        var getPaymentResponse = await _paymentService.GetPaymentAsync(id);
        
        return getPaymentResponse is null
        ? NotFound()
        : Ok(ToResponse(getPaymentResponse));

        // local functions
        GetPaymentResponse ToResponse(Payment payment) => new()
        {
            Id = payment.Id,
            Status = payment.Status,
            CardNumberLastFour = payment.CardNumberLastFour,
            ExpiryMonth = payment.ExpiryMonth,
            ExpiryYear = payment.ExpiryYear,
            Currency = payment.Currency,
            Amount = payment.Amount
        };
    }
}
