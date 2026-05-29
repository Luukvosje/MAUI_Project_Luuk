using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeOn.Mobile.Interfaces;

namespace TimeOn.Mobile.Features.Settings.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IAuthenticationService _authenticationService;

    public SettingsViewModel(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _authenticationService.LogoutAsync();

        if (Shell.Current is AppShell appShell)
        {
            await appShell.OnLoggedOutAsync();
        }
    }
}
