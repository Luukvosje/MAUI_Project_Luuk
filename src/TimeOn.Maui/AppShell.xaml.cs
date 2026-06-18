using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TimeOn.Maui.Features.Authentication.Views;
using TimeOn.Maui.Features.Customers.Views;
using TimeOn.Maui.Features.Dashboard.Views;
using TimeOn.Maui.Features.Settings.Views;
using TimeOn.Maui.Features.Tracking.Views;
using TimeOn.Maui.Features.Trips.Views;
using TimeOn.Maui.Features.Tracking.Services;
using TimeOn.Maui.Interfaces;

namespace TimeOn.Maui;

public partial class AppShell : Shell
{
    private readonly ILogger<AppShell> _logger;
    private readonly IAuthenticationService _authenticationService;
    private readonly ITrackingGpsStore _trackingGpsStore;
    private readonly IGpsTrackingService _gpsTrackingService;
    private readonly IDevelopmentModeService _developmentModeService;
    private readonly IGpsNotificationSettingsService _gpsNotificationSettingsService;
    private readonly IAuthSessionCoordinator _sessionCoordinator;
    private readonly IServiceProvider _serviceProvider;

    public AppShell(
        ILogger<AppShell> logger,
        IAuthenticationService authenticationService,
        ITrackingGpsStore trackingGpsStore,
        IGpsTrackingService gpsTrackingService,
        IDevelopmentModeService developmentModeService,
        IGpsNotificationSettingsService gpsNotificationSettingsService,
        IAuthSessionCoordinator sessionCoordinator,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _authenticationService = authenticationService;
        _trackingGpsStore = trackingGpsStore;
        _gpsTrackingService = gpsTrackingService;
        _developmentModeService = developmentModeService;
        _gpsNotificationSettingsService = gpsNotificationSettingsService;
        _sessionCoordinator = sessionCoordinator;
        _serviceProvider = serviceProvider;

        InitializeComponent();
        _sessionCoordinator.SessionExpired += OnSessionExpired;
    }

    public void ConfigurePages()
    {
        LoginShell.Content = _serviceProvider.GetRequiredService<LoginPage>();
        DashboardShell.Content = _serviceProvider.GetRequiredService<DashboardPage>();
        TripsShell.Content = _serviceProvider.GetRequiredService<TripsPage>();
        CustomersShell.Content = _serviceProvider.GetRequiredService<CustomersPage>();
        TrackingShell.Content = _serviceProvider.GetRequiredService<TrackingPage>();
        SettingsShell.Content = _serviceProvider.GetRequiredService<SettingsPage>();
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

        await _developmentModeService.InitializeAsync();
        await _gpsNotificationSettingsService.InitializeAsync();
        await InitializeTrackingAsync();

        if (_authenticationService.IsAuthenticated)
        {
            await RunStartupSyncAsync();
        }
    }

    private async Task InitializeTrackingAsync()
    {
        try
        {
            await _trackingGpsStore.InitializeAsync();
            await _gpsTrackingService.ResumeIfActiveAsync();
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to initialize local tracking store or resume active session.");
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

    private async void OnSessionExpired(object? sender, EventArgs e)
    {
        try
        {
            await OnLoggedOutAsync();
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to navigate to login after session expiry.");
        }
    }

    private async Task RunStartupSyncAsync()
    {
    }
}
