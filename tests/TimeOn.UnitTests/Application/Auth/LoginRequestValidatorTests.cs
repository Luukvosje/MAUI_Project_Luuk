using FluentAssertions;
using TimeOn.Application.Features.Auth.DTOs;
using TimeOn.Application.Features.Auth.Validators;

namespace TimeOn.UnitTests.Application.Auth;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void Validate_WithValidRequest_Succeeds()
    {
        var result = _validator.Validate(new LoginRequestDto("user@example.com", "password123"));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "password123")]
    [InlineData("user@example.com", "short")]
    public void Validate_WithInvalidRequest_Fails(string email, string password)
    {
        var result = _validator.Validate(new LoginRequestDto(email, password));

        result.IsValid.Should().BeFalse();
    }
}
