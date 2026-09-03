using ECommerce.Application.DTOs.Checkout;
using ECommerce.Application.Validators.Orders;
using FluentValidation;

namespace ECommerce.Application.Validators.Checkout;

public class CheckoutRequestValidator : AbstractValidator<CheckoutRequestDto>
{
    public CheckoutRequestValidator()
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer name is required.")
            .MaximumLength(150).WithMessage("Customer name must not exceed 150 characters.");

        RuleFor(x => x.CustomerEmail)
            .NotEmpty().WithMessage("Customer email is required.")
            .EmailAddress().WithMessage("Invalid customer email address.");

        RuleFor(x => x.CustomerPhone)
            .NotEmpty().WithMessage("Customer phone is required.")
            .MaximumLength(30).WithMessage("Phone number must not exceed 30 characters.");

        RuleFor(x => x.ShippingAddress)
            .NotNull().WithMessage("Shipping address is required.")
            .SetValidator(new CreateOrderAddressValidator());

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("Invalid payment method.");

        RuleFor(x => x.PaymentProvider)
            .NotEmpty().WithMessage("Payment provider is required.");
    }
}

public class CheckoutQuoteRequestValidator : AbstractValidator<CheckoutQuoteRequestDto>
{
    public CheckoutQuoteRequestValidator()
    {
        RuleFor(x => x.CountryCode)
            .NotEmpty().WithMessage("Country code is required.")
            .Length(2).WithMessage("Country code must be 2-letter ISO code.");
    }
}
