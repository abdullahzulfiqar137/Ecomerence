using ECommerce.Domain.Common;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class TrackingEvent : BaseEntity
{
    public Guid ShipmentId { get; set; }
    public Shipment? Shipment { get; set; }

    public ShipmentStatus Status { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime EventTimeUtc { get; set; } = DateTime.UtcNow;
}
