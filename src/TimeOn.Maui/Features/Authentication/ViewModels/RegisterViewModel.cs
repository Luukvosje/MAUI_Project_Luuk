using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeOn.Maui.Features.Authentication.Views;
using TimeOn.Maui.Interfaces;

namespace TimeOn.Maui.Features.Authentication.ViewModels;

public partial class RegisterViewModel : ObservableObject
{
    private readonly IAuthenticationService _authenticationService;

    [ObservableProperty]
    public partial string Email { get; set; }

    [ObservableProperty]
    public partial string Password { get; set; }

    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    public RegisterViewModel(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
        Email = string.Empty;
        Password = string.Empty;
        Name = string.Empty;
    }

    [RelayCommand]
    private async Task GoToLoginAsync()
    {
        await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var success = await _authenticationService.RegisterAsync(Name, Email, Password);
            if (!success)
            {
                ErrorMessage = _authenticationService.ErrorMessage ?? "Registratie mislukt.";
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
