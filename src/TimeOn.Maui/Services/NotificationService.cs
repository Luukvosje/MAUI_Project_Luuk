using TimeOn.Maui.Interfaces;

namespace TimeOn.Maui.Services;

public partial class NotificationService : INotificationService
{
    public Task ShowLocalNotificationAsync(string title, string message)
    {
        return ShowNotificationCoreAsync(title, message);
    }

    private partial Task ShowNotificationCoreAsync(string title, string message);
}
