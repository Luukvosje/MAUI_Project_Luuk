using FluentValidation;
using TimeOn.Application.Features.Customers.DTOs;
using TimeOn.Domain.Constants;

namespace TimeOn.Application.Features.Customers.Validators;

public sealed class CreateCustomerRequestValidator : AbstractValidator<CreateCustomerRequestDto>
{
    public CreateCustomerRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(TrackingConstants.MaxCustomerNameLength);

        RuleFor(x => x.Address)
            .MaximumLength(TrackingConstants.MaxCustomerAddressLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Address));

        RuleFor(x => x.ContactEmail)
            .EmailAddress()
            .MaximumLength(TrackingConstants.MaxCustomerContactEmailLength)
            .When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));
    }
}
