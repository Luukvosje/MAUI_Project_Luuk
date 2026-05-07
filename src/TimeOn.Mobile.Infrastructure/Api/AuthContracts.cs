namespace TimeOn.Mobile.Infrastructure.Api;

internal sealed record LoginRequest(string Email, string Password);

internal sealed record LoginResponse(string AccessToken, DateTimeOffset ExpiresAtUtc);
