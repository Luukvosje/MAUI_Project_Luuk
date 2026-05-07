namespace TimeOn.Mobile.Core.Interfaces;

public interface INotificationService
{
    Task ShowAsync(string title, string message, CancellationToken cancellationToken = default);
}
