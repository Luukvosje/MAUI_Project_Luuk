namespace TimeOn.Maui.Interfaces;

public interface IDevelopmentModeService
{
    bool IsSupported { get; }

    bool IsEnabled { get; }

    event EventHandler? Changed;

    Task InitializeAsync();

    Task SetEnabledAsync(bool enabled);
}
