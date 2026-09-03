using ECommerce.Domain.Common;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class TaxRule : AuditableEntity
{
    public string Name { get; set; } = string.Empty; // e.g. "PK Sales Tax", "US NY State Tax", "EU VAT Standard"
    public string CountryCode { get; set; } = string.Empty;
    public string? StateOrProvince { get; set; }
    public string? PostalCode { get; set; }

    public decimal Rate { get; set; } // e.g. 0.17 for 17%
    public TaxCalculationType Type { get; set; } = TaxCalculationType.Percentage;
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; } = 1;
}
