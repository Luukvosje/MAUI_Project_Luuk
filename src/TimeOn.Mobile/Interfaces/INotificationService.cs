namespace TimeOn.Mobile.Interfaces;

public interface INotificationService
{
    Task ShowLocalNotificationAsync(string title, string message);
}
