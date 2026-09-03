using ECommerce.Application.DTOs.Analytics;
using FluentValidation;

namespace ECommerce.Application.Validators.Analytics;

public class SalesTrendRequestValidator : AbstractValidator<SalesTrendRequestDto>
{
    public SalesTrendRequestValidator()
    {
        RuleFor(x => x.GroupBy)
            .Must(g => string.IsNullOrEmpty(g) || g == "day" || g == "month" || g == "year")
            .WithMessage("GroupBy must be one of: 'day', 'month', 'year'.");

        RuleFor(x => x)
            .Must(x => !x.StartDateUtc.HasValue || !x.EndDateUtc.HasValue || x.StartDateUtc.Value <= x.EndDateUtc.Value)
            .WithMessage("StartDate must be earlier than or equal to EndDate.");
    }
}
