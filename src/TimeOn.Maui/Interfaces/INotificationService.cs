namespace TimeOn.Maui.Interfaces;

public interface INotificationService
{
    Task ShowLocalNotificationAsync(string title, string message);
}
