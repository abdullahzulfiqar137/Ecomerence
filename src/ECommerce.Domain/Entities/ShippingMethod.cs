using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class ShippingMethod : AuditableEntity
{
    public string Name { get; set; } = string.Empty; // "Standard Shipping", "Express Shipping", "Same-Day Delivery"
    public string? Code { get; set; } // "STD", "EXP", "SAMEDAY"
    public string? Description { get; set; }
    public string? EstimatedDeliveryDays { get; set; } // "3-5 Business Days"
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;

    public ICollection<ShippingRate> ShippingRates { get; set; } = new List<ShippingRate>();
}
