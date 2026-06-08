using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeOn.Mobile.Interfaces;

namespace TimeOn.Mobile.Features.Settings.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IDevelopmentModeService _developmentModeService;
    private readonly IGpsNotificationSettingsService _gpsNotificationSettingsService;
    private bool _suppressDevelopmentModeSave;
    private bool _suppressGpsNotificationSave;

    public bool IsDevelopmentModeSupported => _developmentModeService.IsSupported;

    [ObservableProperty]
    public partial bool IsDevelopmentModeEnabled { get; set; }

    [ObservableProperty]
    public partial bool AreGpsSaveNotificationsEnabled { get; set; }

    public SettingsViewModel(
        IAuthenticationService authenticationService,
        IDevelopmentModeService developmentModeService,
        IGpsNotificationSettingsService gpsNotificationSettingsService)
    {
        _authenticationService = authenticationService;
        _developmentModeService = developmentModeService;
        _gpsNotificationSettingsService = gpsNotificationSettingsService;
        LoadDevelopmentMode();
        LoadGpsNotificationSettings();
    }

    partial void OnIsDevelopmentModeEnabledChanged(bool value)
    {
        if (_suppressDevelopmentModeSave || !_developmentModeService.IsSupported)
        {
            return;
        }

        _ = _developmentModeService.SetEnabledAsync(value);
    }

    partial void OnAreGpsSaveNotificationsEnabledChanged(bool value)
    {
        if (_suppressGpsNotificationSave)
        {
            return;
        }

        _ = _gpsNotificationSettingsService.SetEnabledAsync(value);
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

    private void LoadGpsNotificationSettings()
    {
        _suppressGpsNotificationSave = true;
        AreGpsSaveNotificationsEnabled = _gpsNotificationSettingsService.IsEnabled;
        _suppressGpsNotificationSave = false;
    }
}
