using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class ProductAttribute : AuditableEntity
{
    public string Name { get; set; } = string.Empty; // e.g., "Size", "Color", "Material"
    public string? Description { get; set; }

    public ICollection<ProductAttributeValue> Values { get; set; } = new List<ProductAttributeValue>();
}
