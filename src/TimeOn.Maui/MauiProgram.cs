using Microsoft.Extensions.Logging;
using TimeOn.Maui.Extensions;

namespace TimeOn.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        SQLitePCL.Batteries_V2.Init();

        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();
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
