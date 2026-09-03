using ECommerce.Domain.Common;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class DiscountRule : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public DiscountType Type { get; set; } = DiscountType.Percentage;
    public decimal Value { get; set; }
    public decimal? MinSubtotal { get; set; }
    public decimal? MaxDiscount { get; set; }
    public bool AppliesToAllProducts { get; set; } = true;
    public bool IsActive { get; set; } = true;
}
