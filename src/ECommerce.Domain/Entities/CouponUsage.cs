using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class CouponUsage : BaseEntity
{
    public Guid CouponId { get; set; }
    public Coupon? Coupon { get; set; }

    public Guid OrderId { get; set; }
    public Order? Order { get; set; }

    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public decimal DiscountApplied { get; set; }
    public DateTime UsedAtUtc { get; set; } = DateTime.UtcNow;
}
