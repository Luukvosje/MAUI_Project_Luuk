using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeOn.Maui.Features.Authentication.Views;
using TimeOn.Maui.Interfaces;

namespace TimeOn.Maui.Features.Authentication.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthenticationService _authenticationService;

    [ObservableProperty]
    public partial string Email { get; set; }

    [ObservableProperty]
    public partial string Password { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    public LoginViewModel(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
        Email = string.Empty;
        Password = string.Empty;
    }

    [RelayCommand]
    private async Task GoToRegisterAsync()
    {
        await Shell.Current.GoToAsync(nameof(RegisterPage));
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var success = await _authenticationService.LoginAsync(Email, Password);
            if (!success)
            {
                ErrorMessage = _authenticationService.ErrorMessage ?? "Inloggen mislukt.";
                return;
            }

            if (Shell.Current is AppShell appShell)
            {
                await appShell.OnAuthenticatedAsync();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
