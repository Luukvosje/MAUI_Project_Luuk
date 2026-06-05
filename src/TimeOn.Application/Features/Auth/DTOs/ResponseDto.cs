namespace TimeOn.Application.Features.Auth.DTOs;

public sealed record LoginResponseDto(
    string AccessToken,
    DateTime ExpiresAt,
    string RefreshToken,
    DateTime RefreshExpiresAt);

public sealed record RegisterResponseDto(
    string AccessToken,
    DateTime ExpiresAt,
    string RefreshToken,
    DateTime RefreshExpiresAt);
