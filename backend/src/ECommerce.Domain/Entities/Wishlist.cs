using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class Wishlist : AuditableEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string Name { get; set; } = "Default Wishlist";
    public bool IsPublic { get; set; } = false;
    public string? ShareToken { get; set; }

    public ICollection<WishlistItem> Items { get; set; } = new List<WishlistItem>();
}
