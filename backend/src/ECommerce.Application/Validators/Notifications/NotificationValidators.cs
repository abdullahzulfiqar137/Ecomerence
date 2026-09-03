using ECommerce.Application.DTOs.Notifications;
using FluentValidation;

namespace ECommerce.Application.Validators.Notifications;

public class SendEmailRequestValidator : AbstractValidator<SendEmailRequestDto>
{
    public SendEmailRequestValidator()
    {
        RuleFor(x => x.RecipientEmail)
            .NotEmpty().WithMessage("Recipient email is required.")
            .EmailAddress().WithMessage("Invalid recipient email address.");

        RuleFor(x => x.RecipientName)
            .NotEmpty().WithMessage("Recipient name is required.")
            .MaximumLength(150).WithMessage("Recipient name must not exceed 150 characters.");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Email subject is required.")
            .MaximumLength(200).WithMessage("Email subject must not exceed 200 characters.");

        RuleFor(x => x.BodyHtml)
            .NotEmpty().WithMessage("Email body HTML is required.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid notification type.");
    }
}
