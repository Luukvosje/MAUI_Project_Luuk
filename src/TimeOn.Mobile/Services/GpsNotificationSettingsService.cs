using TimeOn.Mobile.Configuration;
using TimeOn.Mobile.Interfaces;

namespace TimeOn.Mobile.Services;

public sealed class GpsNotificationSettingsService : IGpsNotificationSettingsService
{
    private readonly ILocalStorageService _localStorageService;

    public GpsNotificationSettingsService(ILocalStorageService localStorageService)
    {
        _localStorageService = localStorageService;
    }

    public bool IsEnabled { get; private set; }

    public event EventHandler? Changed;

    public async Task InitializeAsync()
    {
        IsEnabled = await _localStorageService.GetAsync<bool>(StorageKeys.GpsSaveNotificationsEnabled);
    }

    public async Task SetEnabledAsync(bool enabled)
    {
        if (IsEnabled == enabled)
        {
            return;
        }

        IsEnabled = enabled;
        await _localStorageService.SetAsync(StorageKeys.GpsSaveNotificationsEnabled, enabled);
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
