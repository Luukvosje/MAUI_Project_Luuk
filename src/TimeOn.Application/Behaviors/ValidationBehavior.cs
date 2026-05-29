using FluentValidation;

namespace TimeOn.Application.Behaviors;

public static class ValidationBehavior
{
    public static async Task ValidateAsync<T>(
        T request,
        IValidator<T> validator)
    {
        var result = await validator.ValidateAsync(request);
        if (!result.IsValid)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.ErrorMessage));
            throw new ValidationException(errors);
        }
    }
}
