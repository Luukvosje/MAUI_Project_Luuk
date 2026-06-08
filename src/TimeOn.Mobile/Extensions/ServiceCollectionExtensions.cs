using System.Reflection;
using Microsoft.Extensions.Configuration;
using TimeOn.Mobile.Caching;
using TimeOn.Mobile.Features.Authentication.ViewModels;
using TimeOn.Mobile.Features.Authentication.Views;
using TimeOn.Mobile.Features.Customers.Services;
using TimeOn.Mobile.Features.Customers.ViewModels;
using TimeOn.Mobile.Features.Customers.Views;
using TimeOn.Mobile.Features.Dashboard.ViewModels;
using TimeOn.Mobile.Features.Dashboard.Views;
using TimeOn.Mobile.Features.Settings.ViewModels;
using TimeOn.Mobile.Features.Settings.Views;
using TimeOn.Mobile.Features.Tracking.ViewModels;
using TimeOn.Mobile.Features.Tracking.Views;
using TimeOn.Mobile.Features.Trips.ViewModels;
using TimeOn.Mobile.Features.Trips.Views;
using TimeOn.Mobile.Features.Tracking.Services;
using TimeOn.Mobile.Http;
using TimeOn.Mobile.Interfaces;
using TimeOn.Mobile.Services;
using TimeOn.Mobile.Sync;
using TimeOn.Application.Features.Customers.Services;
using TimeOn.Application.Features.WorkSessions.Services;
#if ANDROID
using TimeOn.Mobile.Platforms.Android;
#endif

namespace TimeOn.Mobile.Extensions;

public static class ServiceCollectionExtensions
{
    public static MauiAppBuilder AddMobileServices(this MauiAppBuilder builder)
    {
        var configuration = BuildConfiguration();
        builder.Services.AddSingleton(configuration);

        builder.Services.AddSingleton<ICacheStore, MemoryCacheStore>();
        builder.Services.AddSingleton<ISyncQueue, SyncQueue>();

        builder.Services.AddSingleton<ILocalStorageService, LocalStorageService>();
        builder.Services.AddSingleton<IDevelopmentModeService, DevelopmentModeService>();
        builder.Services.AddSingleton<IGpsNotificationSettingsService, GpsNotificationSettingsService>();
        builder.Services.AddSingleton<INotificationService, NotificationService>();
        builder.Services.AddSingleton<DrivingStateDetector>();
        builder.Services.AddSingleton<SqliteTrackingStore>();
        builder.Services.AddSingleton<ITrackingGpsStore>(sp => sp.GetRequiredService<SqliteTrackingStore>());
        builder.Services.AddSingleton<IGpsTrackingService, GpsTrackingService>();
#if ANDROID
        builder.Services.AddSingleton<IPlatformLocationTracker, AndroidPlatformLocationTracker>();
#else
        builder.Services.AddSingleton<IPlatformLocationTracker, PollingLocationTracker>();
#endif
        builder.Services.AddSingleton<ISyncService, SyncService>();

        var apiBaseUrl = ResolveApiBaseUrl(configuration["TimeOnApi:BaseUrl"]);

        builder.Services.AddSingleton<IAuthSessionCoordinator, AuthSessionCoordinator>();
        builder.Services.AddSingleton<IAuthTokenStore, AuthTokenStore>();
        builder.Services.AddSingleton<ITokenRefreshService, TokenRefreshService>();
        builder.Services.AddTransient<BearerTokenRefreshingHandler>();

        builder.Services.AddHttpClient("TimeOn.Auth", client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        builder.Services.AddHttpClient<IApiService, ApiService>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        }).AddHttpMessageHandler<BearerTokenRefreshingHandler>();

        builder.Services.AddScoped<ICustomerService, RemoteCustomerService>();
        builder.Services.AddScoped<IWorkSessionService, RemoteWorkSessionService>();
        builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
        builder.Services.AddSingleton<ICustomersMapPresentationService, CustomersMapPresentationService>();

        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<TripsViewModel>();
        builder.Services.AddTransient<TripDetailViewModel>();
        builder.Services.AddTransient<CustomersMapViewModel>();
        builder.Services.AddTransient<CustomersViewModel>();
        builder.Services.AddTransient<CustomerFormViewModel>();
        builder.Services.AddTransient<TrackingViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();

        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<TripsPage>();
        builder.Services.AddTransient<TripDetailPage>();
        builder.Services.AddTransient<CustomersPage>();
        builder.Services.AddTransient<CustomerFormPage>();
        builder.Services.AddTransient<TrackingPage>();
        builder.Services.AddTransient<SettingsPage>();

        builder.Services.AddSingleton<AppShell>();

        return builder;
    }

    private static IConfiguration BuildConfiguration()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var builder = new ConfigurationBuilder();

        var stream = assembly.GetManifestResourceStream("TimeOn.Mobile.appsettings.json");
        if (stream is not null)
        {
            builder.AddJsonStream(stream);
        }

#if ANDROID
        var androidStream = assembly.GetManifestResourceStream("TimeOn.Mobile.appsettings.android.json");
        if (androidStream is not null)
        {
            builder.AddJsonStream(androidStream);
        }
#else
        var platformSuffix = GetPlatformConfigurationSuffix();
        if (platformSuffix is not null)
        {
            var platformResourceName = $"TimeOn.Mobile.appsettings.{platformSuffix}.json";
            var platformStream = assembly.GetManifestResourceStream(platformResourceName);
            if (platformStream is not null)
            {
                builder.AddJsonStream(platformStream);
            }
        }
#endif

        if (!builder.Sources.Any())
        {
            builder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["TimeOnApi:BaseUrl"] = "https://localhost:5001/"
            });
        }

        return builder.Build();
    }

    private static string ResolveApiBaseUrl(string? configured)
    {
        var baseUrl = configured ?? "https://localhost:5001/";

#if ANDROID
        if (baseUrl.Contains("localhost", StringComparison.OrdinalIgnoreCase))
        {
            baseUrl = "http://10.0.2.2:5000/";
        }
#endif

        return baseUrl.EndsWith('/') ? baseUrl : baseUrl + "/";
    }

    private static string? GetPlatformConfigurationSuffix()
    {
        if (OperatingSystem.IsAndroid())
        {
            return "android";
        }

        if (OperatingSystem.IsWindows())
        {
            return "windows";
        }

        if (OperatingSystem.IsIOS())
        {
            return "ios";
        }

        return null;
    }
}
