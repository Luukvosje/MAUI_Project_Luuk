using TimeOn.Mobile.Core.Interfaces;
using TimeOn.Mobile.Infrastructure.Storage;

namespace TimeOn.Mobile.Infrastructure.Api;

public sealed class AuthService : IAuthService
{
    private readonly ApiClient apiClient;
    private readonly JwtTokenStore tokenStore;
    private readonly IKeyValueStore keyValueStore;
    private const string TokenKey = "auth_token";

    public AuthService(ApiClient apiClient, JwtTokenStore tokenStore, IKeyValueStore keyValueStore)
    {
        this.apiClient = apiClient;
        this.tokenStore = tokenStore;
        this.keyValueStore = keyValueStore;
    }

    public async Task<bool> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        LoginResponse? response;
        try
        {
            response = await apiClient.PostAsJsonAsync<LoginRequest, LoginResponse>(
                "api/auth/login",
                new LoginRequest(email, password),
                cancellationToken);
        }
        catch (HttpRequestException)
        {
            return false;
        }

        if (response?.AccessToken is null)
        {
            return false;
        }

        string token = response.AccessToken;
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
