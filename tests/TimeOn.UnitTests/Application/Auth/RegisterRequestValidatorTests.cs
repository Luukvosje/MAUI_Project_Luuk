using FluentAssertions;
using TimeOn.Application.Features.Auth.DTOs;
using TimeOn.Application.Features.Auth.Validators;

namespace TimeOn.UnitTests.Application.Auth;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    [Fact]
    public void Validate_WithValidRequest_Succeeds()
    {
        var result = _validator.Validate(
            new RegisterRequestDto("User", "user@example.com", "password123", "password123"));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithMismatchedPasswords_Fails()
    {
        var result = _validator.Validate(
            new RegisterRequestDto("User", "user@example.com", "password123", "different"));

        result.IsValid.Should().BeFalse();
    }
}
