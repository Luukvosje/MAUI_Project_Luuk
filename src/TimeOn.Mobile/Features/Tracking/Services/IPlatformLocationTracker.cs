using TimeOn.Mobile.Features.Tracking.Models;

namespace TimeOn.Mobile.Features.Tracking.Services;

public interface IPlatformLocationTracker
{
    Task StartAsync(Func<LocationReading, Task> onReading);

    Task StopAsync();
}
