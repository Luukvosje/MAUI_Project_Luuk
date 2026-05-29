using TimeOn.Application.Features.Auth.DTOs;
using TimeOn.Mobile.Interfaces;

namespace TimeOn.Mobile.Services;

public sealed class AuthenticationService : IAuthenticationService
{
    public const string AuthTokenKey = "auth_token";
    private const string LoginEndpoint = "api/auth/login";
    private const string RegisterEndpoint = "api/auth/register";

    private readonly IApiService _apiService;
    private readonly ILocalStorageService _localStorageService;

    public AuthenticationService(
        IApiService apiService,
        ILocalStorageService localStorageService)
    {
        _apiService = apiService;
        _localStorageService = localStorageService;
    }

    public bool IsAuthenticated { get; private set; }

    public string? ErrorMessage { get; private set; }

    public async Task InitializeAsync()
    {
        var token = await _localStorageService.GetAsync<string>(AuthTokenKey);
        IsAuthenticated = !string.IsNullOrWhiteSpace(token);
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
                ErrorMessage = _apiService.LastError ?? "Login failed.";
                IsAuthenticated = false;
                return false;
            }

            await _localStorageService.SetAsync(AuthTokenKey, response.AccessToken);
            IsAuthenticated = true;
            return true;
        }
        catch (Exception)
        {
            ErrorMessage = _apiService.LastError ?? "Login failed.";
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
                ErrorMessage = _apiService.LastError ?? "Registration failed.";
                IsAuthenticated = false;
                return false;
            }

            await _localStorageService.SetAsync(AuthTokenKey, response.AccessToken);
            IsAuthenticated = true;
            return true;
        }
        catch (Exception)
        {
            ErrorMessage = _apiService.LastError ?? "Registration failed.";
            IsAuthenticated = false;
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        await _localStorageService.RemoveAsync(AuthTokenKey);
        IsAuthenticated = false;
        ErrorMessage = null;
    }
}
