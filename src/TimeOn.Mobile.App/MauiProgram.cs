using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using TimeOn.Mobile.App.Services;
using TimeOn.Mobile.App.ViewModels;
using TimeOn.Mobile.App.Views;
using TimeOn.Mobile.Core.Interfaces;
using TimeOn.Mobile.Core.UseCases;
using TimeOn.Mobile.Infrastructure.Api;
using TimeOn.Mobile.Infrastructure.Repositories;
using TimeOn.Mobile.Infrastructure.Storage;
using TimeOn.Mobile.Infrastructure.Device;

namespace TimeOn.Mobile.App;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		using Stream appSettingsStream = FileSystem.OpenAppPackageFileAsync("appsettings.json").GetAwaiter().GetResult();
		builder.Configuration.AddJsonStream(appSettingsStream);

		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		var apiOptions = new ApiOptions
		{
			BaseUrl = builder.Configuration["Api:BaseUrl"] ?? "https://localhost:7000/"
		};
		builder.Services.AddSingleton(Options.Create(apiOptions));
		builder.Services.AddSingleton<JwtTokenStore>();
		builder.Services.AddSingleton<IKeyValueStore, SecurePreferencesStore>();
		builder.Services.AddSingleton<IAuthService, AuthService>();
		builder.Services.AddSingleton<ILocationService, MauiLocationService>();
		builder.Services.AddSingleton<INotificationService, MauiNotificationService>();
		builder.Services.AddSingleton<ITrackingService, TrackingService>();
		builder.Services.AddSingleton<ITripRepository, InMemoryTripRepository>();
		builder.Services.AddSingleton<ICustomerVisitRepository, InMemoryCustomerVisitRepository>();

		builder.Services.AddSingleton(sp =>
		{
			var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ApiOptions>>().Value;
			return new HttpClient { BaseAddress = new Uri(options.BaseUrl) };
		});
		builder.Services.AddSingleton<ApiClient>();

		builder.Services.AddSingleton<TripDistanceCalculator>();
		builder.Services.AddSingleton<StandstillDetector>();
		builder.Services.AddSingleton<CustomerSuggestionEngine>();

		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<DashboardViewModel>();
		builder.Services.AddTransient<TripsOverviewViewModel>();
		builder.Services.AddTransient<VisitsViewModel>();

		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<DashboardPage>();
		builder.Services.AddTransient<TripsOverviewPage>();
		builder.Services.AddTransient<VisitsPage>();

		builder.Services.AddSingleton<AppShell>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
