using ECommerce.Domain.Common;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class Shipment : AuditableEntity
{
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }

    public string CarrierName { get; set; } = string.Empty; // "DHL", "FedEx", "TCS", "Leopards", "Standard"
    public string TrackingNumber { get; set; } = string.Empty;
    public string? TrackingUrl { get; set; }
    public ShipmentStatus Status { get; set; } = ShipmentStatus.Pending;

    public DateTime? ShippedAtUtc { get; set; }
    public DateTime? EstimatedDeliveryUtc { get; set; }
    public DateTime? DeliveredAtUtc { get; set; }

    public decimal ShippingCost { get; set; } = 0m;
    public string? Notes { get; set; }

    public ICollection<ShipmentItem> Items { get; set; } = new List<ShipmentItem>();
    public ICollection<TrackingEvent> TrackingEvents { get; set; } = new List<TrackingEvent>();
}
