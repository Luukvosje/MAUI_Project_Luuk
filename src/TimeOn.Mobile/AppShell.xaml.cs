using Microsoft.Extensions.Logging;
using TimeOn.Mobile.Features.Authentication.Views;
using TimeOn.Mobile.Features.Customers.Views;
using TimeOn.Mobile.Features.Dashboard.Views;
using TimeOn.Mobile.Features.Settings.Views;
using TimeOn.Mobile.Features.Tracking.Views;
using TimeOn.Mobile.Features.Trips.Views;
using TimeOn.Mobile.Interfaces;

namespace TimeOn.Mobile;

public partial class AppShell : Shell
{
    private readonly ILogger<AppShell> _logger;
    private readonly IAuthenticationService _authenticationService;
    private readonly ISyncService _syncService;

    public AppShell(
        ILogger<AppShell> logger,
        IAuthenticationService authenticationService,
        ISyncService syncService,
        LoginPage loginPage,
        DashboardPage dashboardPage,
        TripsPage tripsPage,
        CustomersPage customersPage,
        TrackingPage trackingPage,
        SettingsPage settingsPage)
    {
        _logger = logger;
        _authenticationService = authenticationService;
        _syncService = syncService;

        InitializeComponent();

        LoginShell.Content = loginPage;
        DashboardShell.Content = dashboardPage;
        TripsShell.Content = tripsPage;
        CustomersShell.Content = customersPage;
        TrackingShell.Content = trackingPage;
        SettingsShell.Content = settingsPage;
    }

    public async Task InitializeAsync()
    {
        try
        {
            await _authenticationService.InitializeAsync();
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to initialize authentication state. Falling back to unauthenticated shell.");
        }

        await MainThread.InvokeOnMainThreadAsync(ApplyAuthState);

        if (_authenticationService.IsAuthenticated)
        {
            await RunStartupSyncAsync();
        }
    }

    public void ApplyAuthState()
    {
        CurrentItem = _authenticationService.IsAuthenticated ? MainTabBar : LoginShell;
    }

    public async Task OnAuthenticatedAsync()
    {
        await MainThread.InvokeOnMainThreadAsync(ApplyAuthState);
        await GoToAsync($"//{nameof(DashboardPage)}");
    }

    public async Task OnLoggedOutAsync()
    {
        await MainThread.InvokeOnMainThreadAsync(ApplyAuthState);
        await GoToAsync($"//{nameof(LoginPage)}");
    }

    private async Task RunStartupSyncAsync()
    {
        try
        {
            await _syncService.SyncPendingChangesAsync();
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Startup customer sync failed; continuing with local data.");
        }
    }
}
