using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.DTOs.Wishlist;
using FluentValidation;

namespace ECommerce.Application.Validators.Cart;

public class AddToCartValidator : AbstractValidator<AddToCartRequestDto>
{
    public AddToCartValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be at least 1.")
            .LessThanOrEqualTo(100).WithMessage("Maximum quantity per item is 100.");
    }
}

public class UpdateCartItemValidator : AbstractValidator<UpdateCartItemRequestDto>
{
    public UpdateCartItemValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative.")
            .LessThanOrEqualTo(100).WithMessage("Maximum quantity per item is 100.");
    }
}

public class AddToWishlistValidator : AbstractValidator<AddToWishlistRequestDto>
{
    public AddToWishlistValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product is required.");
    }
}
