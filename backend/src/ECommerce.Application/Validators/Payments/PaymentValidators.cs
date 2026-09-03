using ECommerce.Application.DTOs.Payments;
using FluentValidation;

namespace ECommerce.Application.Validators.Payments;

public class CreatePaymentIntentValidator : AbstractValidator<CreatePaymentIntentRequestDto>
{
    public CreatePaymentIntentValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.Provider)
            .NotEmpty().WithMessage("Payment provider is required.")
            .MaximumLength(50).WithMessage("Provider must not exceed 50 characters.");

        RuleFor(x => x.MethodType)
            .IsInEnum().WithMessage("Invalid payment method.");
    }
}

public class RefundRequestValidator : AbstractValidator<RefundRequestDto>
{
    public RefundRequestValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty().WithMessage("Payment ID is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Refund amount must be greater than zero.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Refund reason is required.")
            .MaximumLength(300).WithMessage("Reason must not exceed 300 characters.");
    }
}
