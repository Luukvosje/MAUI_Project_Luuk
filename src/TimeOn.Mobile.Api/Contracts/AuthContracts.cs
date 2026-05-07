namespace TimeOn.Mobile.Api.Contracts;

public sealed record LoginRequest(string Email, string Password);

public sealed record LoginResponse(string AccessToken, DateTimeOffset ExpiresAtUtc);
