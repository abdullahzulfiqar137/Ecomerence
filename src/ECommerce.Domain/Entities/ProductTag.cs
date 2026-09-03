using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class ProductTag : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public string Tag { get; set; } = string.Empty;
}
