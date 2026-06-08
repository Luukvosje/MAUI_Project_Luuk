using Microsoft.Extensions.DependencyInjection;

namespace TimeOn.Mobile;

public partial class App : Microsoft.Maui.Controls.Application
{
    private AppShell? _appShell;

    public App()
    {
        InitializeComponent();
        UserAppTheme = AppTheme.Light;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var appShell = Handler?.MauiContext?.Services.GetRequiredService<AppShell>()
            ?? throw new InvalidOperationException("Unable to resolve AppShell from the MAUI service provider.");

        appShell.ConfigurePages();
        _appShell = appShell;

        var window = new Window(appShell);
        appShell.Loaded += OnAppShellLoaded;
        return window;
    }

    private async void OnAppShellLoaded(object? sender, EventArgs e)
    {
        if (_appShell is null)
        {
            return;
        }

        _appShell.Loaded -= OnAppShellLoaded;

        try
        {
            await _appShell.InitializeAsync();
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);
        }
    }
}
