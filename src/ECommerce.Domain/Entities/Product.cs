using ECommerce.Domain.Common;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class Product : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? FullDescription { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public ProductType Type { get; set; } = ProductType.Simple;

    // Pricing
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public decimal? CostPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal TaxRate { get; set; } = 0m;

    // Inventory Controls
    public bool TrackInventory { get; set; } = true;
    public int MinimumOrderQuantity { get; set; } = 1;
    public int? MaximumOrderQuantity { get; set; }
    public int LowStockThreshold { get; set; } = 5;

    // Physical Specs
    public decimal? Weight { get; set; }
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }

    // Flags
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; } = false;
    public bool IsNew { get; set; } = true;
    public bool IsBestSeller { get; set; } = false;

    // Ratings Aggregate (Cached on entity for fast sorting/filtering)
    public decimal AverageRating { get; set; } = 0m;
    public int TotalReviews { get; set; } = 0;

    // SEO
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public string? CanonicalUrl { get; set; }

    // Relationships
    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }

    public Guid? BrandId { get; set; }
    public Brand? Brand { get; set; }

    // Navigation
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<ProductVideo> Videos { get; set; } = new List<ProductVideo>();
    public ICollection<ProductSpecification> Specifications { get; set; } = new List<ProductSpecification>();
    public ICollection<ProductTag> Tags { get; set; } = new List<ProductTag>();
    public ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
    public ICollection<ProductQuestion> Questions { get; set; } = new List<ProductQuestion>();
    public ICollection<ProductRelatedProduct> RelatedProducts { get; set; } = new List<ProductRelatedProduct>();
    public ICollection<ProductBundleItem> BundleItems { get; set; } = new List<ProductBundleItem>();
    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
}
