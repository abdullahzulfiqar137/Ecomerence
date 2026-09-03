using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class ShipmentItem : BaseEntity
{
    public Guid ShipmentId { get; set; }
    public Shipment? Shipment { get; set; }

    public Guid OrderItemId { get; set; }
    public OrderItem? OrderItem { get; set; }

    public int Quantity { get; set; }
}
