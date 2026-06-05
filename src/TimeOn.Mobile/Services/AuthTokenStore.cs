using TimeOn.Mobile.Interfaces;

namespace TimeOn.Mobile.Services;

public sealed class AuthTokenStore : IAuthTokenStore
{
    public const string AccessTokenKey = AuthenticationService.AuthTokenKey;
    public const string RefreshTokenKey = AuthenticationService.RefreshTokenKey;

    private readonly ILocalStorageService _localStorageService;

    public AuthTokenStore(ILocalStorageService localStorageService)
    {
        _localStorageService = localStorageService;
    }

    public Task<string?> GetAccessTokenAsync() =>
        _localStorageService.GetAsync<string>(AccessTokenKey);

    public Task<string?> GetRefreshTokenAsync() =>
        _localStorageService.GetAsync<string>(RefreshTokenKey);

    public async Task SaveTokensAsync(string accessToken, string refreshToken)
    {
        await _localStorageService.SetAsync(AccessTokenKey, accessToken);
        await _localStorageService.SetAsync(RefreshTokenKey, refreshToken);
    }

    public async Task ClearAsync()
    {
        await _localStorageService.RemoveAsync(AccessTokenKey);
        await _localStorageService.RemoveAsync(RefreshTokenKey);
    }
}
