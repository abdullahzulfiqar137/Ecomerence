using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class ProductVideo : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public string VideoUrl { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int DisplayOrder { get; set; } = 0;
}
