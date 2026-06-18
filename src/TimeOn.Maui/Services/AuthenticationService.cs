using TimeOn.Application.Features.Auth.DTOs;
using TimeOn.Maui.Interfaces;

namespace TimeOn.Maui.Services;

public sealed class AuthenticationService : IAuthenticationService
{
    public const string AuthTokenKey = "auth_token";
    public const string RefreshTokenKey = "refresh_token";
    private const string LoginEndpoint = "api/auth/login";
    private const string RegisterEndpoint = "api/auth/register";

    private readonly IApiService _apiService;
    private readonly IAuthTokenStore _tokenStore;
    private readonly ITokenRefreshService _tokenRefreshService;
    private readonly IAuthSessionCoordinator _sessionCoordinator;

    public AuthenticationService(
        IApiService apiService,
        IAuthTokenStore tokenStore,
        ITokenRefreshService tokenRefreshService,
        IAuthSessionCoordinator sessionCoordinator)
    {
        _apiService = apiService;
        _tokenStore = tokenStore;
        _tokenRefreshService = tokenRefreshService;
        _sessionCoordinator = sessionCoordinator;
        _sessionCoordinator.SessionExpired += OnSessionExpired;
    }

    public bool IsAuthenticated { get; private set; }

    public string? ErrorMessage { get; private set; }

    public async Task InitializeAsync()
    {
        var accessToken = await _tokenStore.GetAccessTokenAsync();
        var refreshToken = await _tokenStore.GetRefreshTokenAsync();

        if (string.IsNullOrWhiteSpace(accessToken) && string.IsNullOrWhiteSpace(refreshToken))
        {
            IsAuthenticated = false;
            return;
        }

        if (!string.IsNullOrWhiteSpace(accessToken) && !JwtPayloadReader.IsExpired(accessToken, TimeSpan.FromMinutes(1)))
        {
            IsAuthenticated = true;
            return;
        }

        IsAuthenticated = await _tokenRefreshService.TryRefreshAsync();
        if (!IsAuthenticated)
        {
            await _tokenStore.ClearAsync();
        }
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        ErrorMessage = null;

        try
        {
            var response = await _apiService.PostAsync<LoginRequestDto, LoginResponseDto>(
                LoginEndpoint,
                new LoginRequestDto(email, password));

            if (response is null)
            {
                ErrorMessage = _apiService.LastError ?? "Inloggen mislukt.";
                IsAuthenticated = false;
                return false;
            }

            await SaveTokensAsync(response);
            IsAuthenticated = true;
            return true;
        }
        catch (Exception)
        {
            ErrorMessage = _apiService.LastError ?? "Inloggen mislukt.";
            IsAuthenticated = false;
            return false;
        }
    }

    public async Task<bool> RegisterAsync(string name, string email, string password)
    {
        ErrorMessage = null;

        try
        {
            var response = await _apiService.PostAsync<RegisterRequestDto, RegisterResponseDto>(
                RegisterEndpoint,
                new RegisterRequestDto(name, email, password, password));

            if (response is null)
            {
                ErrorMessage = _apiService.LastError ?? "Registratie mislukt.";
                IsAuthenticated = false;
                return false;
            }

            await SaveTokensAsync(response);
            IsAuthenticated = true;
            return true;
        }
        catch (Exception)
        {
            ErrorMessage = _apiService.LastError ?? "Registratie mislukt.";
            IsAuthenticated = false;
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        await _tokenStore.ClearAsync();
        IsAuthenticated = false;
        ErrorMessage = null;
    }

    private Task SaveTokensAsync(LoginResponseDto response) =>
        _tokenStore.SaveTokensAsync(response.AccessToken, response.RefreshToken);

    private Task SaveTokensAsync(RegisterResponseDto response) =>
        _tokenStore.SaveTokensAsync(response.AccessToken, response.RefreshToken);

    private void OnSessionExpired(object? sender, EventArgs e)
    {
        IsAuthenticated = false;
        ErrorMessage = null;
    }
}
