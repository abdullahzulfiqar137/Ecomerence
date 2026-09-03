using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class Currency : AuditableEntity
{
    public string Code { get; set; } = string.Empty; // "USD", "PKR", "EUR", "GBP", "AED", "SAR", "CAD", "AUD"
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = "$";
    public decimal ExchangeRateToBase { get; set; } = 1.0m; // Relative to Base Currency (USD = 1.0)
    public bool IsBaseCurrency { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public int DecimalPlaces { get; set; } = 2;
}
