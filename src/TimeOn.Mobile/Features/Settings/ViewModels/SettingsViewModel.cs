using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeOn.Mobile.Interfaces;

namespace TimeOn.Mobile.Features.Settings.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IDevelopmentModeService _developmentModeService;
    private bool _suppressDevelopmentModeSave;

    public bool IsDevelopmentModeSupported => _developmentModeService.IsSupported;

    [ObservableProperty]
    public partial bool IsDevelopmentModeEnabled { get; set; }

    public SettingsViewModel(
        IAuthenticationService authenticationService,
        IDevelopmentModeService developmentModeService)
    {
        _authenticationService = authenticationService;
        _developmentModeService = developmentModeService;
        LoadDevelopmentMode();
    }

    partial void OnIsDevelopmentModeEnabledChanged(bool value)
    {
        if (_suppressDevelopmentModeSave || !_developmentModeService.IsSupported)
        {
            return;
        }

        _ = _developmentModeService.SetEnabledAsync(value);
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

    private void LoadDevelopmentMode()
    {
        _suppressDevelopmentModeSave = true;
        IsDevelopmentModeEnabled = _developmentModeService.IsEnabled;
        _suppressDevelopmentModeSave = false;
    }
}
