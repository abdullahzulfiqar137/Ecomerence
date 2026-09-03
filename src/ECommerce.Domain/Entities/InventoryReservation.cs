using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class InventoryReservation : BaseEntity
{
    public Guid InventoryItemId { get; set; }
    public InventoryItem? InventoryItem { get; set; }

    public Guid? OrderId { get; set; }
    public Order? Order { get; set; }

    public string? CartSessionId { get; set; }
    public int Quantity { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public bool IsReleased { get; set; } = false;
    public bool IsCommitted { get; set; } = false;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
