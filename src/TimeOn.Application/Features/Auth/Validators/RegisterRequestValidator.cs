using FluentValidation;
using TimeOn.Application.Features.Auth.DTOs;
using TimeOn.Domain.Constants;

namespace TimeOn.Application.Features.Auth.Validators;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(AuthConstants.MaxUserNameLength);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(AuthConstants.MaxEmailLength);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(AuthConstants.MinPasswordLength)
            .MaximumLength(AuthConstants.MaxPasswordLength);

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password)
            .WithMessage("Passwords do not match.");
    }
}
