#if !ANDROID && !WINDOWS
namespace TimeOn.Maui.Services;

public partial class NotificationService
{
    private partial Task ShowNotificationCoreAsync(string title, string message) =>
        Task.CompletedTask;
}
#endif
