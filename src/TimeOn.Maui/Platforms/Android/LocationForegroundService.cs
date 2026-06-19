#if ANDROID
#pragma warning disable CS8602, CS8603
using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Util;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using TimeOn.Domain.Constants;
using TimeOn.Maui.Features.Tracking.Models;
using TimeOn.Maui.Features.Tracking.Services;

namespace TimeOn.Maui.Platforms.Android;

[Service(
    Exported = false,
    Name = "com.companyname.TimeOn.Maui.LocationForegroundService",
    ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeLocation)]
public sealed class LocationForegroundService : Service, ILocationListener
{
    private const string LogTag = "TimeOnLocationService";
    private const int NotificationId = 10_001;
    private const string ChannelId = "timeon_location_tracking";
    private LocationManager? _locationManager;

    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        CreateNotificationChannel();
        var notification = BuildNotification();
        StartForeground(NotificationId, notification);

        if (ContextCompat.CheckSelfPermission(this, global::Android.Manifest.Permission.AccessFineLocation)
            != global::Android.Content.PM.Permission.Granted)
        {
            Log.Error(LogTag, "Fine location permission is not granted; GPS updates will not be collected.");
            return StartCommandResult.NotSticky;
        }

        _locationManager = (LocationManager?)GetSystemService(LocationService);
        if (_locationManager is null)
        {
            Log.Error(LogTag, "LocationManager is unavailable.");
            return StartCommandResult.NotSticky;
        }

        var minTimeMs = TrackingOptions.FastIntervalSeconds * 1000;
        const float minDistanceM = 0f;
        var looper = Looper.MainLooper
            ?? throw new InvalidOperationException("Main looper is unavailable for location updates.");

        var providersRegistered = 0;
        foreach (var provider in new[] { LocationManager.GpsProvider, LocationManager.NetworkProvider })
        {
            if (!_locationManager.IsProviderEnabled(provider))
            {
                Log.Warn(LogTag, "Location provider '{0}' is disabled.", provider);
                continue;
            }

            _locationManager.RequestLocationUpdates(
                provider,
                minTimeMs,
                minDistanceM,
                this,
                looper);
            providersRegistered++;
            Log.Info(LogTag, "Registered location updates for provider '{0}'.", provider);
        }

        if (providersRegistered == 0)
        {
            Log.Error(LogTag, "No enabled location providers found.");
            return StartCommandResult.NotSticky;
        }

        PushLastKnownLocations();

        return StartCommandResult.Sticky;
    }

    public override void OnDestroy()
    {
        if (_locationManager is not null)
        {
            _locationManager.RemoveUpdates(this);
            _locationManager = null;
        }

        StopForeground(StopForegroundFlags.Remove);
        base.OnDestroy();
    }

    public override IBinder? OnBind(Intent? intent) => null;

    public void OnLocationChanged(global::Android.Locations.Location location)
    {
        var reading = new LocationReading(
            location.Latitude,
            location.Longitude,
            location.HasAccuracy ? location.Accuracy : LocationReading.UnknownAccuracy,
            location.HasSpeed ? location.Speed : 0d,
            DateTimeOffset.FromUnixTimeMilliseconds(location.Time).UtcDateTime);

        Log.Debug(
            LogTag,
            "Location update received: lat={0:F6}, lon={1:F6}, accuracy={2:F1}m",
            reading.Latitude,
            reading.Longitude,
            reading.Accuracy);

        var handler = LocationTrackingBridge.OnReading;
        if (handler is null)
        {
            Log.Warn(LogTag, "Location update received but no tracking handler is registered.");
            return;
        }

        _ = InvokeHandlerSafelyAsync(handler, reading);
    }

    private void PushLastKnownLocations()
    {
        if (_locationManager is null)
        {
            return;
        }

        foreach (var provider in new[] { LocationManager.GpsProvider, LocationManager.NetworkProvider })
        {
            if (!_locationManager.IsProviderEnabled(provider))
            {
                continue;
            }

            var location = _locationManager.GetLastKnownLocation(provider);
            if (location is null)
            {
                continue;
            }

            Log.Info(LogTag, "Pushing last known location from provider '{0}'.", provider);
            OnLocationChanged(location);
            return;
        }

        Log.Warn(LogTag, "No last known location available from any provider.");
    }

    private static async Task InvokeHandlerSafelyAsync(
        Func<LocationReading, Task> handler,
        LocationReading reading)
    {
        try
        {
            await handler(reading).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            Log.Error(LogTag, $"Failed to process location update: {exception}");
        }
    }

    public void OnProviderDisabled(string provider)
    {
    }

    public void OnProviderEnabled(string provider)
    {
    }

    public void OnStatusChanged(string? provider, Availability status, Bundle? extras)
    {
    }

    private void CreateNotificationChannel()
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.O)
        {
            return;
        }

        var manager = (NotificationManager?)GetSystemService(NotificationService);
        if (manager?.GetNotificationChannel(ChannelId) is not null)
        {
            return;
        }

        var channel = new NotificationChannel(
            ChannelId,
            "GPS Tracking",
            NotificationImportance.Low)
        {
            Description = "Shows while a work session is being tracked."
        };

        manager?.CreateNotificationChannel(channel);
    }

    private Notification BuildNotification()
    {
        var launchIntent = PackageManager?.GetLaunchIntentForPackage(PackageName ?? string.Empty);
        var pendingIntent = PendingIntent.GetActivity(
            this,
            0,
            launchIntent,
            PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent);

        var iconId = ApplicationInfo?.Icon ?? global::Android.Resource.Drawable.IcDialogInfo;
        var compatBuilder = new NotificationCompat.Builder(this, ChannelId)!;
        compatBuilder
            .SetContentTitle("TimeOn tracking active")
            .SetContentText("Collecting GPS points for your work session")
            .SetSmallIcon(iconId)
            .SetOngoing(true)
            .SetContentIntent(pendingIntent);

        var notification = compatBuilder.Build();
        return notification ?? throw new InvalidOperationException("Failed to build tracking notification.");
    }
}
#endif
