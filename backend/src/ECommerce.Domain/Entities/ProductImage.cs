using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class ProductImage : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public Guid? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }

    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public bool IsMain { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;
}
