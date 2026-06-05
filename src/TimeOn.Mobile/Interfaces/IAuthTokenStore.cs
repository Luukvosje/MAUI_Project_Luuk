namespace TimeOn.Mobile.Interfaces;

public interface IAuthTokenStore
{
    Task<string?> GetAccessTokenAsync();

    Task<string?> GetRefreshTokenAsync();

    Task SaveTokensAsync(string accessToken, string refreshToken);

    Task ClearAsync();
}
