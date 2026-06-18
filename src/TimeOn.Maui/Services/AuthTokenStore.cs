using Microsoft.Extensions.Logging;
using TimeOn.Maui.Interfaces;

namespace TimeOn.Maui.Services;

public sealed class AuthTokenStore : IAuthTokenStore
{
    public const string AccessTokenKey = AuthenticationService.AuthTokenKey;
    public const string RefreshTokenKey = AuthenticationService.RefreshTokenKey;

    private readonly ILogger<AuthTokenStore> _logger;

    public AuthTokenStore(ILogger<AuthTokenStore> logger)
    {
        _logger = logger;
    }

    public Task<string?> GetAccessTokenAsync() =>
        GetSecureAsync(AccessTokenKey);

    public Task<string?> GetRefreshTokenAsync() =>
        GetSecureAsync(RefreshTokenKey);

    public async Task SaveTokensAsync(string accessToken, string refreshToken)
    {
        try
        {
            await SetSecureAsync(AccessTokenKey, accessToken);
            await SetSecureAsync(RefreshTokenKey, refreshToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to save JWT tokens to SecureStorage.");
            throw new InvalidOperationException("Kon authenticatietokens niet veilig opslaan.", exception);
        }
    }

    public Task ClearAsync()
    {
        RemoveSecure(AccessTokenKey);
        RemoveSecure(RefreshTokenKey);
        return Task.CompletedTask;
    }

    private static async Task<string?> GetSecureAsync(string key)
    {
        try
        {
            return await SecureStorage.Default.GetAsync(key);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static async Task SetSecureAsync(string key, string value)
    {
        await SecureStorage.Default.SetAsync(key, value);
    }

    private static void RemoveSecure(string key)
    {
        try
        {
            SecureStorage.Default.Remove(key);
        }
        catch (Exception)
        {
            Console.WriteLine($"Failed to remove JWT token from SecureStorage: {key}");
        }
    }
}
