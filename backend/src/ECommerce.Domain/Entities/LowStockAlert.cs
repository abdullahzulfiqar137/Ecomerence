using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class LowStockAlert : BaseEntity
{
    public Guid InventoryItemId { get; set; }
    public InventoryItem? InventoryItem { get; set; }

    public int CurrentQuantity { get; set; }
    public int Threshold { get; set; }
    public bool IsResolved { get; set; } = false;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAtUtc { get; set; }
}
