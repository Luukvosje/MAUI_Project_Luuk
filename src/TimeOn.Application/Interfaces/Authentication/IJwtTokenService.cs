namespace TimeOn.Application.Interfaces.Authentication;

public interface IJwtTokenService
{
    AuthToken GenerateAccessToken(Guid userId, string email, string role);

    AuthToken GenerateRefreshToken(Guid userId, string email, string role);

    RefreshTokenPrincipal? ValidateRefreshToken(string refreshToken);
}
