using TimeOn.Mobile.Core.Interfaces;
using TimeOn.Mobile.Infrastructure.Storage;

namespace TimeOn.Mobile.Infrastructure.Api;

public sealed class AuthService : IAuthService
{
    private readonly JwtTokenStore tokenStore;
    private readonly IKeyValueStore keyValueStore;
    private const string TokenKey = "auth_token";

    public AuthService(JwtTokenStore tokenStore, IKeyValueStore keyValueStore)
    {
        this.tokenStore = tokenStore;
        this.keyValueStore = keyValueStore;
    }

    public async Task<bool> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        string token = "mock-jwt-token";
        tokenStore.Set(token);
        await keyValueStore.SetAsync(TokenKey, token, cancellationToken);
        return true;
    }

    public Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        tokenStore.Set(null);
        return keyValueStore.SetAsync(TokenKey, string.Empty, cancellationToken);
    }
}
