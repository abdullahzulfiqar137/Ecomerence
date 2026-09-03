using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class Country : AuditableEntity
{
    public string IsoCode { get; set; } = string.Empty; // "PK", "US", "GB", "AE", "SA", "CA", "DE"
    public string Name { get; set; } = string.Empty;
    public string? PhonePrefix { get; set; } // "+92", "+1", "+44", "+971"
    public string DefaultCurrency { get; set; } = "USD";
    public bool IsShippingAllowed { get; set; } = true;
    public bool IsBillingAllowed { get; set; } = true;
    public bool IsActive { get; set; } = true;
}
