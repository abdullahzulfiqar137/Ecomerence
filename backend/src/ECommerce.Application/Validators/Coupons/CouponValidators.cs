using ECommerce.Application.DTOs.Coupons;
using ECommerce.Domain.Enums;
using FluentValidation;

namespace ECommerce.Application.Validators.Coupons;

public class CreateCouponValidator : AbstractValidator<CreateCouponDto>
{
    public CreateCouponValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Coupon code is required.")
            .MaximumLength(50).WithMessage("Coupon code must not exceed 50 characters.")
            .Matches("^[A-Z0-9_-]+$").WithMessage("Coupon code must contain only uppercase letters, numbers, hyphens, or underscores.");

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0).WithMessage("Discount value must be greater than zero.");

        When(x => x.DiscountType == DiscountType.Percentage, () =>
        {
            RuleFor(x => x.DiscountValue)
                .LessThanOrEqualTo(100).WithMessage("Percentage discount cannot exceed 100%.");
        });

        RuleFor(x => x.StartsAtUtc)
            .NotEmpty().WithMessage("Start date is required.");

        When(x => x.ExpiresAtUtc.HasValue, () =>
        {
            RuleFor(x => x.ExpiresAtUtc!.Value)
                .GreaterThan(x => x.StartsAtUtc).WithMessage("Expiration date must be after start date.");
        });

        When(x => x.MinimumOrderAmount.HasValue, () =>
        {
            RuleFor(x => x.MinimumOrderAmount!.Value)
                .GreaterThanOrEqualTo(0).WithMessage("Minimum order amount cannot be negative.");
        });

        When(x => x.MaximumDiscountAmount.HasValue, () =>
        {
            RuleFor(x => x.MaximumDiscountAmount!.Value)
                .GreaterThan(0).WithMessage("Maximum discount amount must be greater than zero.");
        });
    }
}

public class UpdateCouponValidator : AbstractValidator<UpdateCouponDto>
{
    public UpdateCouponValidator()
    {
        RuleFor(x => x.DiscountValue)
            .GreaterThan(0).WithMessage("Discount value must be greater than zero.");

        When(x => x.DiscountType == DiscountType.Percentage, () =>
        {
            RuleFor(x => x.DiscountValue)
                .LessThanOrEqualTo(100).WithMessage("Percentage discount cannot exceed 100%.");
        });

        When(x => x.ExpiresAtUtc.HasValue, () =>
        {
            RuleFor(x => x.ExpiresAtUtc!.Value)
                .GreaterThan(x => x.StartsAtUtc).WithMessage("Expiration date must be after start date.");
        });
    }
}

public class ValidateCouponValidator : AbstractValidator<ValidateCouponRequestDto>
{
    public ValidateCouponValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Coupon code is required.");

        RuleFor(x => x.OrderSubtotal)
            .GreaterThan(0).WithMessage("Order subtotal must be greater than zero.");
    }
}
