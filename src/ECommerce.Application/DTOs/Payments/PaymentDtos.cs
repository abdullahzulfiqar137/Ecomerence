using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.Payments;

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public PaymentMethodType MethodType { get; set; }
    public string? ExternalTransactionId { get; set; }
    public string? IdempotencyKey { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public PaymentStatus Status { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class CreatePaymentIntentRequestDto
{
    public Guid OrderId { get; set; }
    public string Provider { get; set; } = "Stripe"; // "Stripe", "CashOnDelivery", "JazzCash", "EasyPaisa", "PayPal"
    public PaymentMethodType MethodType { get; set; } = PaymentMethodType.CreditCard;
    public string? ReturnUrl { get; set; }
}

public class PaymentIntentResponseDto
{
    public string Provider { get; set; } = string.Empty;
    public string? ClientSecret { get; set; }
    public string? TransactionId { get; set; }
    public string? RedirectUrl { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public PaymentStatus Status { get; set; }
    public Dictionary<string, string> ProviderPayload { get; set; } = new();
}

public class RefundRequestDto
{
    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class RefundResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? RefundTransactionId { get; set; }
    public decimal RefundedAmount { get; set; }
    public DateTime RefundedAtUtc { get; set; }
}
