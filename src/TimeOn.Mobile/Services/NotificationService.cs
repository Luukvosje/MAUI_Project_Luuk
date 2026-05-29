using TimeOn.Mobile.Interfaces;

namespace TimeOn.Mobile.Services;

public sealed class NotificationService : INotificationService
{
    public Task ShowLocalNotificationAsync(string title, string message)
    {
        _ = title;
        _ = message;
        
        return Task.CompletedTask;
    }
}
