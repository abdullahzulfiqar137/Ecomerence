using ECommerce.Domain.Common;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class Coupon : AuditableEntity
{
    public string Code { get; set; } = string.Empty; // e.g. "WELCOME10", "EID2026", "FLASH50"
    public string? Description { get; set; }
    public DiscountType DiscountType { get; set; } = DiscountType.Percentage;

    public decimal DiscountValue { get; set; } // e.g. 10 for 10%, or 20 for $20 off
    public decimal? MinimumOrderAmount { get; set; }
    public decimal? MaximumDiscountAmount { get; set; } // Cap on percentage discounts

    public DateTime StartsAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAtUtc { get; set; }

    public int? UsageLimitTotal { get; set; } // Total times coupon can be used across platform
    public int? UsageLimitPerCustomer { get; set; } = 1;
    public int CurrentUsageCount { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<CouponUsage> Usages { get; set; } = new List<CouponUsage>();
}
