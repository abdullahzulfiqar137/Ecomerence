using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.International;

public class CurrencyDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public decimal ExchangeRateToBase { get; set; }
    public bool IsBaseCurrency { get; set; }
    public bool IsActive { get; set; }
    public int DecimalPlaces { get; set; }
}

public class CreateCurrencyDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public decimal ExchangeRateToBase { get; set; } = 1.0m;
    public bool IsBaseCurrency { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public int DecimalPlaces { get; set; } = 2;
}

public class UpdateCurrencyRateDto
{
    public decimal ExchangeRateToBase { get; set; }
}

public class ConvertCurrencyRequestDto
{
    public decimal Amount { get; set; }
    public string FromCurrency { get; set; } = "USD";
    public string ToCurrency { get; set; } = "PKR";
}

public class ConvertCurrencyResponseDto
{
    public decimal OriginalAmount { get; set; }
    public string FromCurrency { get; set; } = string.Empty;
    public decimal ConvertedAmount { get; set; }
    public string ToCurrency { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; }
    public DateTime ConvertedAtUtc { get; set; }
}

public class CountryDto
{
    public Guid Id { get; set; }
    public string IsoCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? PhonePrefix { get; set; }
    public string DefaultCurrency { get; set; } = "USD";
    public bool IsShippingAllowed { get; set; }
    public bool IsBillingAllowed { get; set; }
    public bool IsActive { get; set; }
}

public class CreateCountryDto
{
    public string IsoCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? PhonePrefix { get; set; }
    public string DefaultCurrency { get; set; } = "USD";
    public bool IsShippingAllowed { get; set; } = true;
    public bool IsBillingAllowed { get; set; } = true;
    public bool IsActive { get; set; } = true;
}

public class TaxRuleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string? StateOrProvince { get; set; }
    public string? PostalCode { get; set; }
    public decimal Rate { get; set; }
    public TaxCalculationType Type { get; set; }
    public bool IsActive { get; set; }
    public int Priority { get; set; }
}

public class CreateTaxRuleDto
{
    public string Name { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string? StateOrProvince { get; set; }
    public string? PostalCode { get; set; }
    public decimal Rate { get; set; }
    public TaxCalculationType Type { get; set; } = TaxCalculationType.Percentage;
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; } = 1;
}

public class UpdateTaxRuleDto : CreateTaxRuleDto
{
}

public class CalculateTaxRequestDto
{
    public decimal SubtotalAmount { get; set; }
    public string CountryCode { get; set; } = "PK";
    public string? StateOrProvince { get; set; }
    public string? PostalCode { get; set; }
}

public class CalculateTaxResponseDto
{
    public decimal SubtotalAmount { get; set; }
    public decimal TotalTaxAmount { get; set; }
    public decimal EffectiveTaxRate { get; set; }
    public List<AppliedTaxRuleDetailDto> AppliedRules { get; set; } = new();
}

public class AppliedTaxRuleDetailDto
{
    public string RuleName { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public decimal TaxAmount { get; set; }
}
