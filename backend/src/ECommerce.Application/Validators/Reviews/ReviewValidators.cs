using ECommerce.Application.DTOs.Reviews;
using ECommerce.Domain.Enums;
using FluentValidation;

namespace ECommerce.Application.Validators.Reviews;

public class CreateProductReviewValidator : AbstractValidator<CreateProductReviewDto>
{
    public CreateProductReviewValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5 stars.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Review title is required.")
            .MaximumLength(150).WithMessage("Review title must not exceed 150 characters.");

        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage("Review comment is required.")
            .MaximumLength(2000).WithMessage("Review comment must not exceed 2000 characters.");
    }
}

public class UpdateProductReviewValidator : AbstractValidator<UpdateProductReviewDto>
{
    public UpdateProductReviewValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5 stars.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Review title is required.")
            .MaximumLength(150).WithMessage("Review title must not exceed 150 characters.");

        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage("Review comment is required.")
            .MaximumLength(2000).WithMessage("Review comment must not exceed 2000 characters.");
    }
}

public class ModerateReviewValidator : AbstractValidator<ModerateReviewDto>
{
    public ModerateReviewValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid review status.")
            .Must(s => s == ReviewStatus.Approved || s == ReviewStatus.Rejected)
            .WithMessage("Status must be either Approved or Rejected.");
    }
}

public class AdminReviewResponseValidator : AbstractValidator<AdminReviewResponseDto>
{
    public AdminReviewResponseValidator()
    {
        RuleFor(x => x.Response)
            .NotEmpty().WithMessage("Staff response message is required.")
            .MaximumLength(1000).WithMessage("Response must not exceed 1000 characters.");
    }
}
