#if ANDROID
using Android.Content;
using Android.OS;
using TimeOn.Mobile.Features.Tracking.Models;
using TimeOn.Mobile.Features.Tracking.Services;

namespace TimeOn.Mobile.Platforms.Android;

public sealed class AndroidPlatformLocationTracker : IPlatformLocationTracker
{
    public Task StartAsync(Func<LocationReading, Task> onReading)
    {
        LocationTrackingBridge.OnReading = onReading;

        var context = global::Android.App.Application.Context;
        var intent = new Intent(context, typeof(LocationForegroundService));

        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            context.StartForegroundService(intent);
        }
        else
        {
            context.StartService(intent);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        LocationTrackingBridge.OnReading = null;

        var context = global::Android.App.Application.Context;
        var intent = new Intent(context, typeof(LocationForegroundService));
        context.StopService(intent);

        return Task.CompletedTask;
    }
}
#endif
