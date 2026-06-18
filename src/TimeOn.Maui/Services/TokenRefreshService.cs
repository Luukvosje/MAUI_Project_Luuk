using System.Net.Http.Json;
using System.Text.Json;
using TimeOn.Application.Features.Auth.DTOs;
using TimeOn.Maui.Interfaces;

namespace TimeOn.Maui.Services;

public sealed class TokenRefreshService : ITokenRefreshService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAuthTokenStore _tokenStore;
    private readonly IAuthSessionCoordinator _sessionCoordinator;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    public TokenRefreshService(
        IHttpClientFactory httpClientFactory,
        IAuthTokenStore tokenStore,
        IAuthSessionCoordinator sessionCoordinator)
    {
        _httpClientFactory = httpClientFactory;
        _tokenStore = tokenStore;
        _sessionCoordinator = sessionCoordinator;
    }

    public async Task<bool> TryRefreshAsync()
    {
        await _refreshLock.WaitAsync();
        try
        {
            var refreshToken = await _tokenStore.GetRefreshTokenAsync();
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return false;
            }

            var accessToken = await _tokenStore.GetAccessTokenAsync();
            if (!string.IsNullOrWhiteSpace(accessToken) && !JwtPayloadReader.IsExpired(accessToken, TimeSpan.FromMinutes(1)))
            {
                return true;
            }

            var client = _httpClientFactory.CreateClient("TimeOn.Auth");
            using var response = await client.PostAsJsonAsync(
                "api/auth/refresh",
                new RefreshTokenRequestDto(refreshToken));

            if (!response.IsSuccessStatusCode)
            {
                await ClearSessionAsync();
                return false;
            }

            var tokens = await response.Content.ReadFromJsonAsync<LoginResponseDto>(JsonOptions);
            if (tokens is null
                || string.IsNullOrWhiteSpace(tokens.AccessToken)
                || string.IsNullOrWhiteSpace(tokens.RefreshToken))
            {
                await ClearSessionAsync();
                return false;
            }

            await _tokenStore.SaveTokensAsync(tokens.AccessToken, tokens.RefreshToken);
            return true;
        }
        catch (HttpRequestException)
        {
            return false;
        }
        catch (TaskCanceledException)
        {
            return false;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private async Task ClearSessionAsync()
    {
        await _tokenStore.ClearAsync();
        _sessionCoordinator.NotifySessionExpired();
    }
}
