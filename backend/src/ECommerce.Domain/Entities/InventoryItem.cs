using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class InventoryItem : AuditableEntity
{
    public Guid WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }

    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public Guid? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }

    public Guid? WarehouseLocationId { get; set; }
    public WarehouseLocation? WarehouseLocation { get; set; }

    // Concurrency / Stock Counters
    public int QuantityOnHand { get; set; } = 0;
    public int QuantityReserved { get; set; } = 0;
    public int QuantityAvailable => Math.Max(0, QuantityOnHand - QuantityReserved);
    public int ReorderPoint { get; set; } = 10;
    public int ReorderQuantity { get; set; } = 50;

    // Concurrency Token for optimistic locking on stock operations
    public byte[]? RowVersion { get; set; }

    // Navigation
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
    public ICollection<InventoryReservation> Reservations { get; set; } = new List<InventoryReservation>();
}
