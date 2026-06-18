using TimeOn.Maui.Configuration;
using TimeOn.Maui.Interfaces;

namespace TimeOn.Maui.Services;

public sealed class DevelopmentModeService : IDevelopmentModeService
{
    private readonly ILocalStorageService _localStorageService;

    public DevelopmentModeService(ILocalStorageService localStorageService)
    {
        _localStorageService = localStorageService;
    }

    public bool IsSupported => OperatingSystem.IsWindows();

    public bool IsEnabled { get; private set; }

    public event EventHandler? Changed;

    public async Task InitializeAsync()
    {
        if (!IsSupported)
        {
            IsEnabled = false;
            return;
        }

        IsEnabled = await _localStorageService.GetAsync<bool>(StorageKeys.DevelopmentModeEnabled);
    }

    public async Task SetEnabledAsync(bool enabled)
    {
        if (!IsSupported)
        {
            return;
        }

        if (IsEnabled == enabled)
        {
            return;
        }

        IsEnabled = enabled;
        await _localStorageService.SetAsync(StorageKeys.DevelopmentModeEnabled, enabled);
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
