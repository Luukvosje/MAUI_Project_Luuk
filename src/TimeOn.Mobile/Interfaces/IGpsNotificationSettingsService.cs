namespace TimeOn.Mobile.Interfaces;

public interface IGpsNotificationSettingsService
{
    bool IsEnabled { get; }

    event EventHandler? Changed;

    Task InitializeAsync();

    Task SetEnabledAsync(bool enabled);
}
