#if ANDROID
#pragma warning disable CS8602
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using TimeOn.Mobile.Services;

namespace TimeOn.Mobile.Services;

public partial class NotificationService
{
    private const string AlertChannelId = "timeon_driving_alerts";
    private const int AlertNotificationIdBase = 10_002;

    private static int _nextNotificationId = AlertNotificationIdBase;

    private async partial Task ShowNotificationCoreAsync(string title, string message)
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
        {
            var status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.PostNotifications>();
                if (status != PermissionStatus.Granted)
                {
                    return;
                }
            }
        }

        var context = global::Android.App.Application.Context;
        EnsureAlertChannel(context);

        var launchIntent = context.PackageManager?.GetLaunchIntentForPackage(context.PackageName ?? string.Empty);
        var pendingIntent = PendingIntent.GetActivity(
            context,
            0,
            launchIntent,
            PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent);

        var iconId = context.ApplicationInfo?.Icon ?? global::Android.Resource.Drawable.IcDialogInfo;
        var notificationId = _nextNotificationId++;
        if (_nextNotificationId > AlertNotificationIdBase + 100)
        {
            _nextNotificationId = AlertNotificationIdBase;
        }

        var builder = new NotificationCompat.Builder(context, AlertChannelId)!
            .SetContentTitle(title)
            .SetContentText(message)
            .SetSmallIcon(iconId)
            .SetAutoCancel(true)
            .SetContentIntent(pendingIntent)
            .SetPriority(NotificationCompat.PriorityDefault);

        var manager = NotificationManagerCompat.From(context);
        manager?.Notify(notificationId, builder.Build());
    }

    private static void EnsureAlertChannel(Context context)
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.O)
        {
            return;
        }

        var manager = (NotificationManager?)context.GetSystemService(Context.NotificationService);
        if (manager?.GetNotificationChannel(AlertChannelId) is not null)
        {
            return;
        }

        var channel = new NotificationChannel(
            AlertChannelId,
            "Driving alerts",
            NotificationImportance.Default)
        {
            Description = "Alerts when driving or a stop is detected during tracking."
        };

        manager?.CreateNotificationChannel(channel);
    }
}
#endif
