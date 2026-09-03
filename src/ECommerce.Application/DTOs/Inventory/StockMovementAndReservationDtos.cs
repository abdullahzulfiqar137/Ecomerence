using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.Inventory;

public class StockMovementDto
{
    public Guid Id { get; set; }
    public Guid InventoryItemId { get; set; }
    public StockMovementType Type { get; set; }
    public int Quantity { get; set; }
    public int QuantityBefore { get; set; }
    public int QuantityAfter { get; set; }
    public string? ReferenceType { get; set; }
    public string? ReferenceId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
}

public class InventoryReservationDto
{
    public Guid Id { get; set; }
    public Guid InventoryItemId { get; set; }
    public Guid? OrderId { get; set; }
    public string? CartSessionId { get; set; }
    public int Quantity { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public bool IsExpired => DateTime.UtcNow > ExpiresAtUtc;
    public bool IsReleased { get; set; }
    public bool IsCommitted { get; set; }
}

public class CreateReservationRequestDto
{
    public Guid WarehouseId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public int Quantity { get; set; }
    public int HoldDurationMinutes { get; set; } = 15;
    public string? CartSessionId { get; set; }
    public Guid? OrderId { get; set; }
}

public class LowStockAlertDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;
    public Guid? ProductVariantId { get; set; }
    public string? VariantName { get; set; }
    public string? VariantSKU { get; set; }
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int QuantityAvailable { get; set; }
    public int ReorderPoint { get; set; }
    public int ReorderQuantity { get; set; }
}
