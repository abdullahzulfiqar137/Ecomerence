using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class ShippingRate : BaseEntity
{
    public Guid ShippingZoneId { get; set; }
    public ShippingZone? ShippingZone { get; set; }

    public Guid ShippingMethodId { get; set; }
    public ShippingMethod? ShippingMethod { get; set; }

    public decimal BaseRate { get; set; }
    public decimal RatePerKg { get; set; } = 0m;
    public decimal? FreeShippingThreshold { get; set; } // Order subtotal above which shipping is free
    public string Currency { get; set; } = "USD";
}
