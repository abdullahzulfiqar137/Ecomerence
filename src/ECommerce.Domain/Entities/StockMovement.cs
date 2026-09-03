using ECommerce.Domain.Common;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class StockMovement : BaseEntity
{
    public Guid InventoryItemId { get; set; }
    public InventoryItem? InventoryItem { get; set; }

    public StockMovementType Type { get; set; }
    public int Quantity { get; set; } // Positive or negative
    public int QuantityBefore { get; set; }
    public int QuantityAfter { get; set; }

    public string? ReferenceType { get; set; } // "Order", "ManualAdjustment", "PurchaseOrder", "Return"
    public string? ReferenceId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
}
