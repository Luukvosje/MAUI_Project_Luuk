namespace TimeOn.Application.Interfaces.Authentication;

public sealed record AuthToken(string AccessToken, DateTime ExpiresAt);
