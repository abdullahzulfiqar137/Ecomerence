using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class Cart : AuditableEntity
{
    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public string? GuestSessionId { get; set; } // For unauthenticated guest carts
    public string Currency { get; set; } = "USD";
    public string? AppliedCouponCode { get; set; }
    public decimal DiscountAmount { get; set; } = 0m;
    public DateTime? ExpiresAtUtc { get; set; }

    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
