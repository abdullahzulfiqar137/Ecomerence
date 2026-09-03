using ECommerce.Application.DTOs.Auth;
using FluentValidation;

namespace ECommerce.Application.Validators.Auth;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequestDto>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}
