using FluentValidation;
using TimeOn.Application.Behaviors;
using TimeOn.Domain.Shared;
using TimeOn.Application.Features.Auth.DTOs;
using TimeOn.Application.Interfaces.Authentication;
using TimeOn.Domain.Entities;
using TimeOn.Domain.Exceptions;
using TimeOn.Domain.Interfaces;
using TimeOn.Domain.RepositoryInterfaces;
using TimeOn.Domain.ValueObjects;

namespace TimeOn.Application.Features.Auth.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IValidator<LoginRequestDto> _loginValidator;
    private readonly IValidator<RegisterRequestDto> _registerValidator;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IValidator<LoginRequestDto> loginValidator,
        IValidator<RegisterRequestDto> registerValidator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _loginValidator = loginValidator;
        _registerValidator = registerValidator;
    }

    public async Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto request)
    {
        try
        {
            await ValidationBehavior.ValidateAsync(request, _loginValidator);
        }
        catch (ValidationException ex)
        {
            return Result<LoginResponseDto>.Failure(ex.Message);
        }

        Email email;
        try
        {
            email = Email.Create(request.Email);
        }
        catch (DomainException)
        {
            return Result<LoginResponseDto>.Failure("Invalid email or password.");
        }

        var user = await _userRepository.GetByEmailAsync(email);
        if (user is null)
        {
            return Result<LoginResponseDto>.Failure("Invalid email or password.");
        }

        var authResult = user.Authenticate(request.Password, _passwordHasher);
        if (!authResult.IsSuccess)
        {
            return Result<LoginResponseDto>.Failure("Invalid email or password.");
        }

        var token = _jwtTokenService.GenerateToken(
            user.Id,
            user.Email.Value,
            AuthRoles.Default);

        return Result<LoginResponseDto>.Success(
            new LoginResponseDto(token.AccessToken, token.ExpiresAt));
    }

    public async Task<Result<RegisterResponseDto>> RegisterAsync(RegisterRequestDto request)
    {
        try
        {
            await ValidationBehavior.ValidateAsync(request, _registerValidator);
        }
        catch (ValidationException ex)
        {
            return Result<RegisterResponseDto>.Failure(ex.Message);
        }

        Email email;
        try
        {
            email = Email.Create(request.Email);
        }
        catch (DomainException ex)
        {
            return Result<RegisterResponseDto>.Failure(ex.Message);
        }

        if (await _userRepository.ExistsByEmailAsync(email))
        {
            return Result<RegisterResponseDto>.Failure("An account with this email already exists.");
        }

        User user;
        try
        {
            user = User.Register(
                request.Name,
                request.Email,
                request.Password,
                _passwordHasher);
        }
        catch (DomainException ex)
        {
            return Result<RegisterResponseDto>.Failure(ex.Message);
        }

        await _userRepository.AddAsync(user);

        var token = _jwtTokenService.GenerateToken(
            user.Id,
            user.Email.Value,
            AuthRoles.Default);

        return Result<RegisterResponseDto>.Success(
            new RegisterResponseDto(token.AccessToken, token.ExpiresAt));
    }
}
