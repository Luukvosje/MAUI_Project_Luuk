using TimeOn.Mobile.Interfaces;

namespace TimeOn.Mobile.Services;

public sealed class LocationTrackingService : ILocationTrackingService
{
    public bool IsTracking { get; private set; }

    public Task StartTrackingAsync()
    {
        IsTracking = true;
        return Task.CompletedTask;
    }

    public Task StopTrackingAsync()
    {
        IsTracking = false;
        return Task.CompletedTask;
    }
}
