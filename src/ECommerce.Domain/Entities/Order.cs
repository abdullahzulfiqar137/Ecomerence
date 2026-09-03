using ECommerce.Domain.Common;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class Order : AuditableEntity
{
    // Unique public order number e.g., ORD-2026-000001
    public string OrderNumber { get; set; } = string.Empty;

    // Idempotency token to prevent duplicate order submissions
    public string? IdempotencyKey { get; set; }

    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;

    // Statuses
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public ShipmentStatus ShippingStatus { get; set; } = ShipmentStatus.Pending;

    // Currency & Financials
    public string Currency { get; set; } = "USD";
    public decimal ExchangeRate { get; set; } = 1.0m;

    public decimal SubtotalAmount { get; set; }
    public decimal DiscountAmount { get; set; } = 0m;
    public decimal TaxAmount { get; set; } = 0m;
    public decimal ShippingAmount { get; set; } = 0m;
    public decimal TotalAmount { get; set; }

    public string? AppliedCouponCode { get; set; }
    public string? Notes { get; set; }

    // Addresses
    public Guid ShippingAddressId { get; set; }
    public OrderAddress? ShippingAddress { get; set; }

    public Guid? BillingAddressId { get; set; }
    public OrderAddress? BillingAddress { get; set; }

    // Navigation
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
    public ICollection<OrderNote> OrderNotes { get; set; } = new List<OrderNote>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
    public ICollection<InventoryReservation> Reservations { get; set; } = new List<InventoryReservation>();
}
