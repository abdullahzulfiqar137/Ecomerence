using ECommerce.Domain.Common;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class Payment : AuditableEntity
{
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }

    public string Provider { get; set; } = string.Empty; // "Stripe", "CashOnDelivery", "PayPal", "EasyPaisa", "JazzCash"
    public PaymentMethodType MethodType { get; set; } = PaymentMethodType.CashOnDelivery;
    public string? ExternalTransactionId { get; set; }
    public string? IdempotencyKey { get; set; }

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public DateTime? CompletedAtUtc { get; set; }
    public string? FailureReason { get; set; }
    public string? RawGatewayResponse { get; set; }
}
