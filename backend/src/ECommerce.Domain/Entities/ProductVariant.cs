using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class ProductVariant : AuditableEntity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string? Barcode { get; set; }

    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public decimal? CostPrice { get; set; }

    // Physical Attributes
    public string? Size { get; set; }
    public string? Color { get; set; }
    public string? ColorCode { get; set; } // e.g. Hex code #FFFFFF
    public decimal? Weight { get; set; }
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }

    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;

    // Navigation
    public ICollection<ProductAttributeValue> AttributeValues { get; set; } = new List<ProductAttributeValue>();
    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
}
