using Microsoft.Extensions.Logging;
using TimeOn.Mobile.Extensions;

namespace TimeOn.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();
#if !WINDOWS
        builder.UseMauiMaps();
#endif
        builder.ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        });

        builder.AddMobileServices();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        RoutingExtensions.RegisterRoutes();

        return builder.Build();
    }
}
