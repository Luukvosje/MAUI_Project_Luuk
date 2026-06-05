namespace TimeOn.Application.Interfaces.Authentication;

public sealed record RefreshTokenPrincipal(Guid UserId, string Email, string Role);
