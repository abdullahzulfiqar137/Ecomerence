using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class ProductSpecification : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public string Group { get; set; } = "General"; // e.g. "Technical", "Dimensions", "Material"
    public string Name { get; set; } = string.Empty; // e.g. "Battery Life"
    public string Value { get; set; } = string.Empty; // e.g. "24 Hours"
    public int DisplayOrder { get; set; } = 0;
}
