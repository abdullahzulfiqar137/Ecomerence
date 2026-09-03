using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class ShippingZone : AuditableEntity
{
    public string Name { get; set; } = string.Empty; // e.g. "Domestic - Pakistan", "North America", "Europe", "Middle East"
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // Semicolon/comma separated list or ISO country codes e.g., "PK", "US;CA", "AE;SA;QA"
    public string CountryCodes { get; set; } = string.Empty;

    public ICollection<ShippingRate> ShippingRates { get; set; } = new List<ShippingRate>();
}
