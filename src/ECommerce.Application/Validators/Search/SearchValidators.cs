using ECommerce.Application.DTOs.Search;
using FluentValidation;

namespace ECommerce.Application.Validators.Search;

public class SearchRequestValidator : AbstractValidator<SearchRequestDto>
{
    public SearchRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0).When(x => x.MinPrice.HasValue).WithMessage("Min price cannot be negative.");

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(0).When(x => x.MaxPrice.HasValue).WithMessage("Max price cannot be negative.");

        RuleFor(x => x)
            .Must(x => !x.MinPrice.HasValue || !x.MaxPrice.HasValue || x.MinPrice.Value <= x.MaxPrice.Value)
            .WithMessage("Min price must be less than or equal to Max price.");

        RuleFor(x => x.MinRating)
            .InclusiveBetween(0m, 5m).When(x => x.MinRating.HasValue)
            .WithMessage("Min rating must be between 0 and 5.");
    }
}
