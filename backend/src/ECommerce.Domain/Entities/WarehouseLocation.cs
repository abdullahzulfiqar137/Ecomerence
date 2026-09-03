using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class WarehouseLocation : BaseEntity
{
    public Guid WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }

    public string Aisle { get; set; } = string.Empty; // e.g., "A-01"
    public string Rack { get; set; } = string.Empty;  // e.g., "R-03"
    public string Shelf { get; set; } = string.Empty; // e.g., "S-02"
    public string Bin { get; set; } = string.Empty;   // e.g., "B-15"
    public string Code => $"{Warehouse?.Code}-{Aisle}-{Rack}-{Shelf}-{Bin}";
    public bool IsActive { get; set; } = true;
}
