using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TimeOn.Application.Interfaces.Authentication;
using TimeOn.Infrastructure.Configurations;

namespace TimeOn.Infrastructure.Authentication;

public sealed class JwtTokenService : IJwtTokenService
{
    private const string TokenTypeClaim = "token_type";
    private const string AccessTokenType = "access";
    private const string RefreshTokenType = "refresh";

    private readonly JwtSettings _settings;
    private readonly TokenValidationParameters _validationParameters;

    public JwtTokenService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        _validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _settings.Issuer,
            ValidAudience = _settings.Audience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    }

    public AuthToken GenerateAccessToken(Guid userId, string email, string role) =>
        GenerateToken(userId, email, role, AccessTokenType, TimeSpan.FromMinutes(_settings.ExpirationMinutes));

    public AuthToken GenerateRefreshToken(Guid userId, string email, string role) =>
        GenerateToken(userId, email, role, RefreshTokenType, TimeSpan.FromDays(_settings.RefreshExpirationDays));

    public RefreshTokenPrincipal? ValidateRefreshToken(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return null;
        }

        try
        {
            var principal = new JwtSecurityTokenHandler().ValidateToken(refreshToken, _validationParameters, out _);
            var tokenType = principal.FindFirst(TokenTypeClaim)?.Value;
            if (!string.Equals(tokenType, RefreshTokenType, StringComparison.Ordinal))
            {
                return null;
            }

            var userIdValue = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (!Guid.TryParse(userIdValue, out var userId))
            {
                return null;
            }

            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            var role = principal.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(role))
            {
                return null;
            }

            return new RefreshTokenPrincipal(userId, email, role);
        }
        catch (SecurityTokenException)
        {
            return null;
        }
    }

    private AuthToken GenerateToken(Guid userId, string email, string role, string tokenType, TimeSpan lifetime)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(role);

        var expiresAt = DateTime.UtcNow.Add(lifetime);

        var claims = new List<Claim>
        {
            new(TokenTypeClaim, tokenType),
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        return new AuthToken(accessToken, expiresAt);
    }
}
