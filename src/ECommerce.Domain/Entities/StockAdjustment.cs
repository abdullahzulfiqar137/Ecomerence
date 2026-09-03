using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class StockAdjustment : AuditableEntity
{
    public Guid InventoryItemId { get; set; }
    public InventoryItem? InventoryItem { get; set; }

    public int OldQuantity { get; set; }
    public int NewQuantity { get; set; }
    public int Difference => NewQuantity - OldQuantity;
    public string Reason { get; set; } = string.Empty; // "Audit Correction", "Damaged", "Theft", "Supplier Return"
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
}
