using FluentAssertions;
using TimeOn.Application.Features.Auth.DTOs;
using TimeOn.Application.Features.Auth.Validators;

namespace TimeOn.UnitTests.Application.Auth;

public class AuthValidatorTests
{
    private readonly LoginRequestValidator _loginValidator = new();
    private readonly RegisterRequestValidator _registerValidator = new();

    [Theory]
    [InlineData("user@example.com", "password123", true)]
    [InlineData("", "password123", false)]
    [InlineData("not-an-email", "password123", false)]
    [InlineData("user@example.com", "short", false)]
    public void LoginRequestValidator_EnforcesEmailAndPasswordRules(
        string email,
        string password,
        bool expectedValid)
    {
        var result = _loginValidator.Validate(new LoginRequestDto(email, password));

        result.IsValid.Should().Be(expectedValid);
    }

    [Theory]
    [InlineData("User", "user@example.com", "password123", "password123", true)]
    [InlineData("User", "user@example.com", "password123", "different", false)]
    [InlineData("", "user@example.com", "password123", "password123", false)]
    [InlineData("User", "invalid-email", "password123", "password123", false)]
    public void RegisterRequestValidator_EnforcesRegistrationRules(
        string name,
        string email,
        string password,
        string confirmPassword,
        bool expectedValid)
    {
        var result = _registerValidator.Validate(
            new RegisterRequestDto(name, email, password, confirmPassword));

        result.IsValid.Should().Be(expectedValid);
    }
}
