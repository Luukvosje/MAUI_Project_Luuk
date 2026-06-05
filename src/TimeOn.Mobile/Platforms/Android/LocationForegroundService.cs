#if ANDROID
#pragma warning disable CS8602, CS8603
using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using AndroidX.Core.App;
using TimeOn.Domain.Constants;
using TimeOn.Mobile.Features.Tracking.Models;
using TimeOn.Mobile.Features.Tracking.Services;

namespace TimeOn.Mobile.Platforms.Android;

[Service(
    Exported = false,
    Name = "com.companyname.timeon.mobile.LocationForegroundService",
    ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeLocation)]
public sealed class LocationForegroundService : Service, ILocationListener
{
    private const int NotificationId = 10_001;
    private const string ChannelId = "timeon_location_tracking";
    private LocationManager? _locationManager;

    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        CreateNotificationChannel();
        var notification = BuildNotification();
        StartForeground(NotificationId, notification);

        _locationManager = (LocationManager?)GetSystemService(LocationService);
        if (_locationManager is not null)
        {
            var minTimeMs = TrackingOptions.FastIntervalSeconds * 1000;
            var minDistanceM = (float)TrackingOptions.MinDistanceMeters;

            if (_locationManager.IsProviderEnabled(LocationManager.GpsProvider))
            {
                _locationManager.RequestLocationUpdates(
                    LocationManager.GpsProvider,
                    minTimeMs,
                    minDistanceM,
                    this);
            }

            if (_locationManager.IsProviderEnabled(LocationManager.NetworkProvider))
            {
                _locationManager.RequestLocationUpdates(
                    LocationManager.NetworkProvider,
                    minTimeMs,
                    minDistanceM,
                    this);
            }
        }

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

        var handler = LocationTrackingBridge.OnReading;
        if (handler is not null)
        {
            _ = handler(reading);
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
