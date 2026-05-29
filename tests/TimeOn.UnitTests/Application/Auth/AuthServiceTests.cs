using FluentAssertions;
using FluentValidation;
using NSubstitute;
using ValidationException = FluentValidation.ValidationException;
using TimeOn.Application.Features.Auth.DTOs;
using TimeOn.Application.Features.Auth.Services;
using TimeOn.Application.Features.Auth.Validators;
using TimeOn.Application.Interfaces.Authentication;
using TimeOn.Domain.Entities;
using TimeOn.Domain.Interfaces;
using TimeOn.Domain.RepositoryInterfaces;
using TimeOn.Domain.ValueObjects;

namespace TimeOn.UnitTests.Application.Auth;

public class AuthServiceTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenService _jwtTokenService = Substitute.For<IJwtTokenService>();

    private AuthService CreateSut() =>
        new(
            _userRepository,
            _passwordHasher,
            _jwtTokenService,
            new LoginRequestValidator(),
            new RegisterRequestValidator());

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsToken()
    {
        _passwordHasher.Hash(Arg.Any<string>()).Returns("hashed-password");
        _passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>())
            .Returns(call => call.ArgAt<string>(0) == "password123");

        var email = Email.Create("user@example.com");
        var user = User.Register("Test User", email.Value, "password123", _passwordHasher);
        _userRepository.GetByEmailAsync(Arg.Any<Email>()).Returns(user);
        _jwtTokenService.GenerateToken(user.Id, email.Value, "User")
            .Returns(new AuthToken("token-123", DateTime.UtcNow.AddHours(1)));

        var result = await CreateSut().LoginAsync(new LoginRequestDto(email.Value, "password123"));

        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().Be("token-123");
    }

    [Fact]
    public async Task LoginAsync_WithUnknownEmail_ReturnsFailure()
    {
        var email = Email.Create("missing@example.com");
        _userRepository.GetByEmailAsync(Arg.Any<Email>()).Returns((User?)null);

        var result = await CreateSut().LoginAsync(new LoginRequestDto(email.Value, "password123"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid email or password.");
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ReturnsFailure()
    {
        _passwordHasher.Hash(Arg.Any<string>()).Returns("hashed-password");
        _passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        var email = Email.Create("user@example.com");
        var user = User.Register("Test User", email.Value, "password123", _passwordHasher);
        _userRepository.GetByEmailAsync(Arg.Any<Email>()).Returns(user);

        var result = await CreateSut().LoginAsync(new LoginRequestDto(email.Value, "wrong-password"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid email or password.");
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ReturnsFailure()
    {
        _userRepository.ExistsByEmailAsync(Arg.Any<Email>()).Returns(true);

        var result = await CreateSut().RegisterAsync(
            new RegisterRequestDto("Name", "taken@example.com", "password123", "password123"));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("An account with this email already exists.");
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ReturnsTokenAndPersistsUser()
    {
        var request = new RegisterRequestDto("New User", "new@example.com", "password123", "password123");
        _userRepository.ExistsByEmailAsync(Arg.Any<Email>()).Returns(false);
        _passwordHasher.Hash("password123").Returns("hashed-password");
        _jwtTokenService.GenerateToken(Arg.Any<Guid>(), request.Email, "User")
            .Returns(new AuthToken("register-token", DateTime.UtcNow.AddHours(1)));

        var result = await CreateSut().RegisterAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().Be("register-token");
        await _userRepository.Received(1).AddAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task LoginAsync_WithInvalidEmailFormat_ThrowsValidationException()
    {
        var act = () => CreateSut().LoginAsync(new LoginRequestDto("not-an-email", "password123"));

        await act.Should().ThrowAsync<ValidationException>();
    }
}
