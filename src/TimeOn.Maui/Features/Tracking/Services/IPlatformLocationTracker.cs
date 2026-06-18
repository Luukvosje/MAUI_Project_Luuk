using TimeOn.Maui.Features.Tracking.Models;

namespace TimeOn.Maui.Features.Tracking.Services;

public interface IPlatformLocationTracker
{
    Task StartAsync(Func<LocationReading, Task> onReading);

    Task StopAsync();
}
