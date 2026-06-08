#if WINDOWS
using System.Runtime.InteropServices;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using TimeOn.Mobile.Services;

namespace TimeOn.Mobile.Services;

public partial class NotificationService
{
    private const string AppUserModelId = "com.companyname.timeon.mobile";

    private static bool _isRegistered;

    [DllImport("shell32.dll", SetLastError = true)]
    private static extern void SetCurrentProcessExplicitAppUserModelID(
        [MarshalAs(UnmanagedType.LPWStr)] string appId);

    private partial Task ShowNotificationCoreAsync(string title, string message)
    {
        EnsureRegistered();

        var template = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
        var textNodes = template.GetElementsByTagName("text");

        if (textNodes.Length >= 2)
        {
            textNodes[0].AppendChild(template.CreateTextNode(title));
            textNodes[1].AppendChild(template.CreateTextNode(message));
        }

        var toast = new ToastNotification(template);
        ToastNotificationManager.CreateToastNotifier(AppUserModelId).Show(toast);

        return Task.CompletedTask;
    }

    private static void EnsureRegistered()
    {
        if (_isRegistered)
        {
            return;
        }

        SetCurrentProcessExplicitAppUserModelID(AppUserModelId);
        _isRegistered = true;
    }
}
#endif
