using ECommerce.Application.DTOs.Orders;
using FluentValidation;

namespace ECommerce.Application.Validators.Orders;

public class CreateOrderValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer name is required.")
            .MaximumLength(150).WithMessage("Customer name must not exceed 150 characters.");

        RuleFor(x => x.CustomerEmail)
            .NotEmpty().WithMessage("Customer email is required.")
            .EmailAddress().WithMessage("Invalid email address format.");

        RuleFor(x => x.CustomerPhone)
            .NotEmpty().WithMessage("Customer phone is required.")
            .MaximumLength(30).WithMessage("Phone number must not exceed 30 characters.");

        RuleFor(x => x.ShippingAddress)
            .NotNull().WithMessage("Shipping address is required.")
            .SetValidator(new CreateOrderAddressValidator());

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must contain at least one item.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty().WithMessage("Product is required.");
            item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("Item quantity must be greater than zero.");
        });
    }
}

public class CreateOrderAddressValidator : AbstractValidator<CreateOrderAddressDto>
{
    public CreateOrderAddressValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(30).WithMessage("Phone number must not exceed 30 characters.");

        RuleFor(x => x.StreetLine1)
            .NotEmpty().WithMessage("Street address line 1 is required.")
            .MaximumLength(200).WithMessage("Street address must not exceed 200 characters.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(100).WithMessage("City must not exceed 100 characters.");

        RuleFor(x => x.CountryCode)
            .NotEmpty().WithMessage("Country code is required.")
            .Length(2).WithMessage("Country code must be 2-letter ISO code.");
    }
}

public class UpdateOrderStatusValidator : AbstractValidator<UpdateOrderStatusDto>
{
    public UpdateOrderStatusValidator()
    {
        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("Invalid order status.");
    }
}

public class AddOrderNoteValidator : AbstractValidator<AddOrderNoteDto>
{
    public AddOrderNoteValidator()
    {
        RuleFor(x => x.Note)
            .NotEmpty().WithMessage("Note cannot be empty.")
            .MaximumLength(1000).WithMessage("Note must not exceed 1000 characters.");
    }
}

public class CancelOrderValidator : AbstractValidator<CancelOrderRequestDto>
{
    public CancelOrderValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Cancellation reason is required.")
            .MaximumLength(500).WithMessage("Cancellation reason must not exceed 500 characters.");
    }
}
