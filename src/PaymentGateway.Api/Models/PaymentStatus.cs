using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Models;

/// <summary>
/// The challenge's three payment statuses: Authorized and Declined are stored
/// bank outcomes (201); Rejected is the validation response (400), never stored.
/// Serialized by name, so the wire contract travels with the type rather than
/// depending on host configuration.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentStatus
{
    Authorized,
    Declined,
    Rejected
}