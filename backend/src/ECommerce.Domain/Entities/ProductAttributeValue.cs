using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class ProductAttributeValue : BaseEntity
{
    public Guid ProductAttributeId { get; set; }
    public ProductAttribute? ProductAttribute { get; set; }

    public Guid ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }

    public string Value { get; set; } = string.Empty; // e.g. "XL", "Navy Blue"
}
