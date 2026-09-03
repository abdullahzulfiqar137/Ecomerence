using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class Warehouse : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string StreetLine1 { get; set; } = string.Empty;
    public string? StreetLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string StateOrProvince { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsPrimary { get; set; } = false;

    // Navigation
    public ICollection<WarehouseLocation> Locations { get; set; } = new List<WarehouseLocation>();
    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
}
