using ECommerce.Application.DTOs.Catalog;
using FluentValidation;

namespace ECommerce.Application.Validators.Catalog;

public class CreateVariantValidator : AbstractValidator<CreateVariantDto>
{
    public CreateVariantValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Variant name is required.")
            .MaximumLength(150).WithMessage("Variant name must not exceed 150 characters.");

        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("Variant SKU is required.")
            .MaximumLength(50).WithMessage("Variant SKU must not exceed 50 characters.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative.");
    }
}

public class UpdateVariantValidator : AbstractValidator<UpdateVariantDto>
{
    public UpdateVariantValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Variant name is required.")
            .MaximumLength(150).WithMessage("Variant name must not exceed 150 characters.");

        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("Variant SKU is required.")
            .MaximumLength(50).WithMessage("Variant SKU must not exceed 50 characters.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative.");
    }
}

public class GenerateVariantMatrixValidator : AbstractValidator<GenerateVariantMatrixDto>
{
    public GenerateVariantMatrixValidator()
    {
        RuleFor(x => x.BasePrice)
            .GreaterThanOrEqualTo(0).WithMessage("Base price cannot be negative.");

        RuleFor(x => x.OptionGroups)
            .NotEmpty().WithMessage("At least one option group is required for matrix generation.")
            .Must(groups => groups.All(g => g.Options.Count > 0)).WithMessage("Each option group must have at least one option.");
    }
}
