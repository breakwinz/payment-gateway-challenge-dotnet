using PaymentGateway.Api.Models;

namespace PaymentGateway.Api.Dtos;

/// <summary>
/// 400 body for a request that failed validation:
/// the challenge's Rejected status plus one message per violated rule.
/// </summary>
public record PostPaymentRejectedResponse(PaymentStatus Status, string[] Errors);
