using ECommerce.Application.DTOs.International;
using FluentValidation;

namespace ECommerce.Application.Validators.International;

public class CreateCurrencyValidator : AbstractValidator<CreateCurrencyDto>
{
    public CreateCurrencyValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Currency code is required.")
            .Length(3).WithMessage("Currency code must be exactly 3 uppercase letters.")
            .Matches("^[A-Z]{3}$").WithMessage("Currency code must contain only uppercase letters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Currency name is required.")
            .MaximumLength(100).WithMessage("Currency name must not exceed 100 characters.");

        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("Currency symbol is required.")
            .MaximumLength(10).WithMessage("Symbol must not exceed 10 characters.");

        RuleFor(x => x.ExchangeRateToBase)
            .GreaterThan(0).WithMessage("Exchange rate must be greater than zero.");
    }
}

public class UpdateCurrencyRateValidator : AbstractValidator<UpdateCurrencyRateDto>
{
    public UpdateCurrencyRateValidator()
    {
        RuleFor(x => x.ExchangeRateToBase)
            .GreaterThan(0).WithMessage("Exchange rate must be greater than zero.");
    }
}

public class ConvertCurrencyValidator : AbstractValidator<ConvertCurrencyRequestDto>
{
    public ConvertCurrencyValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0).WithMessage("Amount cannot be negative.");

        RuleFor(x => x.FromCurrency)
            .NotEmpty().WithMessage("From currency code is required.")
            .Length(3).WithMessage("From currency code must be 3 letters.");

        RuleFor(x => x.ToCurrency)
            .NotEmpty().WithMessage("To currency code is required.")
            .Length(3).WithMessage("To currency code must be 3 letters.");
    }
}

public class CreateTaxRuleValidator : AbstractValidator<CreateTaxRuleDto>
{
    public CreateTaxRuleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tax rule name is required.")
            .MaximumLength(150).WithMessage("Tax rule name must not exceed 150 characters.");

        RuleFor(x => x.CountryCode)
            .NotEmpty().WithMessage("Country code is required.")
            .Length(2).WithMessage("Country code must be 2-letter ISO code.");

        RuleFor(x => x.Rate)
            .GreaterThanOrEqualTo(0).WithMessage("Tax rate cannot be negative.")
            .LessThanOrEqualTo(1.0m).WithMessage("Tax rate must be represented as a decimal between 0.0 and 1.0 (e.g. 0.17 for 17%).");
    }
}

public class UpdateTaxRuleValidator : AbstractValidator<UpdateTaxRuleDto>
{
    public UpdateTaxRuleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tax rule name is required.")
            .MaximumLength(150).WithMessage("Tax rule name must not exceed 150 characters.");

        RuleFor(x => x.CountryCode)
            .NotEmpty().WithMessage("Country code is required.")
            .Length(2).WithMessage("Country code must be 2-letter ISO code.");

        RuleFor(x => x.Rate)
            .GreaterThanOrEqualTo(0).WithMessage("Tax rate cannot be negative.")
            .LessThanOrEqualTo(1.0m).WithMessage("Tax rate must be represented as a decimal between 0.0 and 1.0 (e.g. 0.17 for 17%).");
    }
}

public class CalculateTaxValidator : AbstractValidator<CalculateTaxRequestDto>
{
    public CalculateTaxValidator()
    {
        RuleFor(x => x.SubtotalAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Subtotal amount cannot be negative.");

        RuleFor(x => x.CountryCode)
            .NotEmpty().WithMessage("Country code is required.")
            .Length(2).WithMessage("Country code must be 2-letter ISO code.");
    }
}
