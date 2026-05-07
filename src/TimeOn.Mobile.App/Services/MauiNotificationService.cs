using TimeOn.Mobile.Core.Interfaces;

namespace TimeOn.Mobile.App.Services;

public sealed class MauiNotificationService : INotificationService
{
    public Task ShowAsync(string title, string message, CancellationToken cancellationToken = default)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (Shell.Current is not null)
            {
                await Shell.Current.DisplayAlertAsync(title, message, "OK");
            }
        });
        return Task.CompletedTask;
    }
}
