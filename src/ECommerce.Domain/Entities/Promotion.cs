using ECommerce.Domain.Common;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class Promotion : AuditableEntity
{
    public string Name { get; set; } = string.Empty; // e.g. "Summer Sale 2026", "Ramadan Mega Deals"
    public string? Description { get; set; }
    public string? BannerUrl { get; set; }
    public DiscountType DiscountType { get; set; } = DiscountType.Percentage;
    public decimal DiscountValue { get; set; }

    public DateTime StartsAtUtc { get; set; }
    public DateTime EndsAtUtc { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<PromotionProduct> Products { get; set; } = new List<PromotionProduct>();
    public ICollection<PromotionCategory> Categories { get; set; } = new List<PromotionCategory>();
}
