using ECommerce.Application.Models;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.Orders;

public class OrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string? IdempotencyKey { get; set; }
    public Guid? UserId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public ShipmentStatus ShippingStatus { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal ExchangeRate { get; set; }
    public decimal SubtotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? AppliedCouponCode { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public OrderAddressDto ShippingAddress { get; set; } = null!;
    public OrderAddressDto? BillingAddress { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public List<OrderStatusHistoryDto> StatusHistory { get; set; } = new();
    public List<OrderNoteDto> OrderNotes { get; set; } = new();
}

public class OrderItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? VariantName { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalLineAmount { get; set; }
}

public class OrderAddressDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string? Company { get; set; }
    public string? PhoneNumber { get; set; }
    public string StreetLine1 { get; set; } = string.Empty;
    public string? StreetLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string StateOrProvince { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
}

public class OrderStatusHistoryDto
{
    public Guid Id { get; set; }
    public OrderStatus Status { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string? ChangedBy { get; set; }
}

public class OrderNoteDto
{
    public Guid Id { get; set; }
    public string Note { get; set; } = string.Empty;
    public bool IsCustomerVisible { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
}

public class OrderFilterParams : PaginationParams
{
    public OrderStatus? Status { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public ShipmentStatus? ShippingStatus { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public decimal? MinTotal { get; set; }
    public decimal? MaxTotal { get; set; }
    public Guid? CustomerUserId { get; set; }
}
