namespace TimeOn.Maui.Interfaces;

public interface IAuthenticationService
{
    bool IsAuthenticated { get; }

    string? ErrorMessage { get; }

    Task InitializeAsync();

    Task<bool> LoginAsync(string email, string password);

    Task<bool> RegisterAsync(string name, string email, string password);

    Task LogoutAsync();
}
