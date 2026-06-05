using TimeOn.Domain.Shared;
using TimeOn.Application.Features.Auth.DTOs;

namespace TimeOn.Application.Features.Auth.Services;

public interface IAuthService
{
    Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto request);

    Task<Result<RegisterResponseDto>> RegisterAsync(RegisterRequestDto request);

    Task<Result<LoginResponseDto>> RefreshAsync(RefreshTokenRequestDto request);
}
