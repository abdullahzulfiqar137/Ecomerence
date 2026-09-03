using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class WishlistItem : BaseEntity
{
    public Guid WishlistId { get; set; }
    public Wishlist? Wishlist { get; set; }

    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public Guid? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }

    public DateTime AddedAtUtc { get; set; } = DateTime.UtcNow;
}
