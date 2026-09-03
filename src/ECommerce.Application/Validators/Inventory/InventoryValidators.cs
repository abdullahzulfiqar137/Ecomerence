using ECommerce.Application.DTOs.Inventory;
using FluentValidation;

namespace ECommerce.Application.Validators.Inventory;

public class CreateWarehouseValidator : AbstractValidator<CreateWarehouseDto>
{
    public CreateWarehouseValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Warehouse name is required.")
            .MaximumLength(150).WithMessage("Warehouse name must not exceed 150 characters.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Warehouse code is required.")
            .MaximumLength(50).WithMessage("Warehouse code must not exceed 50 characters.");

        RuleFor(x => x.StreetLine1)
            .NotEmpty().WithMessage("Street address is required.")
            .MaximumLength(200).WithMessage("Street address must not exceed 200 characters.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(100).WithMessage("City must not exceed 100 characters.");

        RuleFor(x => x.CountryCode)
            .NotEmpty().WithMessage("Country code is required.")
            .Length(2).WithMessage("Country code must be a 2-letter ISO code.");
    }
}

public class UpdateWarehouseValidator : AbstractValidator<UpdateWarehouseDto>
{
    public UpdateWarehouseValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Warehouse name is required.")
            .MaximumLength(150).WithMessage("Warehouse name must not exceed 150 characters.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Warehouse code is required.")
            .MaximumLength(50).WithMessage("Warehouse code must not exceed 50 characters.");
    }
}

public class AdjustStockValidator : AbstractValidator<AdjustStockRequestDto>
{
    public AdjustStockValidator()
    {
        RuleFor(x => x.WarehouseId)
            .NotEmpty().WithMessage("Warehouse is required.");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product is required.");

        RuleFor(x => x.NewQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Adjustment reason is required.")
            .MaximumLength(250).WithMessage("Reason must not exceed 250 characters.");
    }
}

public class CreateReservationValidator : AbstractValidator<CreateReservationRequestDto>
{
    public CreateReservationValidator()
    {
        RuleFor(x => x.WarehouseId)
            .NotEmpty().WithMessage("Warehouse is required.");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Reservation quantity must be greater than zero.");

        RuleFor(x => x.HoldDurationMinutes)
            .InclusiveBetween(1, 120).WithMessage("Hold duration must be between 1 and 120 minutes.");
    }
}
