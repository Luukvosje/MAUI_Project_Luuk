namespace TimeOn.Mobile;

public partial class App : Microsoft.Maui.Controls.Application
{
    private readonly AppShell _appShell;

    public App(AppShell appShell)
    {
        InitializeComponent();
        _appShell = appShell;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(_appShell);
        _appShell.Loaded += OnAppShellLoaded;
        return window;
    }

    private async void OnAppShellLoaded(object? sender, EventArgs e)
    {
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
