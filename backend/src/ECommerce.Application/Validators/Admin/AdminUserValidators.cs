using ECommerce.Application.DTOs.Admin;
using FluentValidation;

namespace ECommerce.Application.Validators.Admin;

public class AssignUserRolesValidator : AbstractValidator<AssignUserRolesDto>
{
    public AssignUserRolesValidator()
    {
        RuleFor(x => x.RoleNames)
            .NotEmpty().WithMessage("At least one role must be specified.");
    }
}

public class UpdateUserStatusValidator : AbstractValidator<UpdateUserStatusDto>
{
    public UpdateUserStatusValidator()
    {
        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters.");
    }
}
