namespace TimeOn.Mobile.Interfaces;

public interface ILocationTrackingService
{
    Task StartTrackingAsync();
    Task StopTrackingAsync();
    bool IsTracking { get; }
}
