using ECommerce.Application.DTOs.Orders;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.Checkout;

public class CheckoutRequestDto
{
    public string? IdempotencyKey { get; set; }
    public string? GuestSessionId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string Currency { get; set; } = "USD";

    public CreateOrderAddressDto ShippingAddress { get; set; } = null!;
    public CreateOrderAddressDto? BillingAddress { get; set; }

    public Guid? SelectedShippingMethodId { get; set; }
    public PaymentMethodType PaymentMethod { get; set; } = PaymentMethodType.CashOnDelivery;
    public string PaymentProvider { get; set; } = "CashOnDelivery"; // "Stripe", "CashOnDelivery", "JazzCash", "EasyPaisa"
    public string? PaymentToken { get; set; } // Stripe PaymentIntentId / Token or Mock Token
    public string? CouponCode { get; set; }
    public string? Notes { get; set; }
}

public class CheckoutQuoteRequestDto
{
    public string? GuestSessionId { get; set; }
    public string CountryCode { get; set; } = "US";
    public string? StateOrProvince { get; set; }
    public string? PostalCode { get; set; }
    public Guid? SelectedShippingMethodId { get; set; }
    public string? CouponCode { get; set; }
}

public class CheckoutQuoteResponseDto
{
    public decimal SubtotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? AppliedCouponCode { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public int TotalItems { get; set; }
    public List<ShippingMethodOptionDto> AvailableShippingMethods { get; set; } = new();
}

public class ShippingMethodOptionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? EstimatedDeliveryDays { get; set; }
    public decimal Cost { get; set; }
}

public class CheckoutResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string? TransactionId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public OrderDto? Order { get; set; }
}
