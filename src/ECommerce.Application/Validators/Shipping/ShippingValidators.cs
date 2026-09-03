using ECommerce.Application.DTOs.Shipping;
using FluentValidation;

namespace ECommerce.Application.Validators.Shipping;

public class CreateShippingZoneValidator : AbstractValidator<CreateShippingZoneDto>
{
    public CreateShippingZoneValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Zone name is required.")
            .MaximumLength(150).WithMessage("Zone name must not exceed 150 characters.");

        RuleFor(x => x.CountryCodes)
            .NotEmpty().WithMessage("At least one country code is required.");
    }
}

public class CreateShippingMethodValidator : AbstractValidator<CreateShippingMethodDto>
{
    public CreateShippingMethodValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Shipping method name is required.")
            .MaximumLength(150).WithMessage("Shipping method name must not exceed 150 characters.");
    }
}

public class CreateShippingRateValidator : AbstractValidator<CreateShippingRateDto>
{
    public CreateShippingRateValidator()
    {
        RuleFor(x => x.ShippingZoneId)
            .NotEmpty().WithMessage("Shipping zone is required.");

        RuleFor(x => x.ShippingMethodId)
            .NotEmpty().WithMessage("Shipping method is required.");

        RuleFor(x => x.BaseRate)
            .GreaterThanOrEqualTo(0).WithMessage("Base rate cannot be negative.");

        RuleFor(x => x.RatePerKg)
            .GreaterThanOrEqualTo(0).WithMessage("Rate per kg cannot be negative.");
    }
}

public class CreateShipmentValidator : AbstractValidator<CreateShipmentDto>
{
    public CreateShipmentValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.CarrierName)
            .NotEmpty().WithMessage("Carrier name is required.")
            .MaximumLength(100).WithMessage("Carrier name must not exceed 100 characters.");
    }
}

public class AddTrackingEventValidator : AbstractValidator<AddTrackingEventDto>
{
    public AddTrackingEventValidator()
    {
        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required.")
            .MaximumLength(200).WithMessage("Location must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");
    }
}
