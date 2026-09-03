using ECommerce.Application.Models;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.Inventory;

public class InventoryItemDto
{
    public Guid Id { get; set; }
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;
    public Guid? ProductVariantId { get; set; }
    public string? VariantName { get; set; }
    public string? VariantSKU { get; set; }
    public int QuantityOnHand { get; set; }
    public int QuantityReserved { get; set; }
    public int QuantityAvailable { get; set; }
    public int ReorderPoint { get; set; }
    public int ReorderQuantity { get; set; }
    public bool IsLowStock => QuantityAvailable <= ReorderPoint;
}

public class InventoryFilterParams : PaginationParams
{
    public Guid? WarehouseId { get; set; }
    public Guid? ProductId { get; set; }
    public bool? LowStockOnly { get; set; }
}

public class CreateInventoryItemDto
{
    public Guid WarehouseId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public int InitialQuantity { get; set; } = 0;
    public int ReorderPoint { get; set; } = 10;
    public int ReorderQuantity { get; set; } = 50;
}

public class AdjustStockRequestDto
{
    public Guid WarehouseId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public int NewQuantity { get; set; }
    public string Reason { get; set; } = string.Empty; // e.g., "Cycle Count Audit", "Damage Write-off", "Found Stock"
}
