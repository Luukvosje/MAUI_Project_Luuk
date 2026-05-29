namespace TimeOn.Application.Features.Auth.DTOs;

public sealed record LoginRequestDto(string Email, string Password);
public sealed record RegisterRequestDto(string Name, string Email, string Password, string ConfirmPassword);
