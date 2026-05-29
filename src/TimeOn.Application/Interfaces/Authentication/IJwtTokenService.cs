namespace TimeOn.Application.Interfaces.Authentication;

public interface IJwtTokenService
{
    AuthToken GenerateToken(Guid userId, string email, string role);
}
