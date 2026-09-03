using ECommerce.Application.DTOs.Catalog;
using FluentValidation;

namespace ECommerce.Application.Validators.Catalog;

public class CreateProductAttributeValidator : AbstractValidator<CreateProductAttributeDto>
{
    public CreateProductAttributeValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Attribute name is required.")
            .MaximumLength(100).WithMessage("Attribute name must not exceed 100 characters.");
    }
}

public class UpdateProductAttributeValidator : AbstractValidator<UpdateProductAttributeDto>
{
    public UpdateProductAttributeValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Attribute name is required.")
            .MaximumLength(100).WithMessage("Attribute name must not exceed 100 characters.");
    }
}
