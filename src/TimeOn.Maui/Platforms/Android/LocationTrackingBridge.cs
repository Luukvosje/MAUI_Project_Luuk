using TimeOn.Maui.Features.Tracking.Models;

namespace TimeOn.Maui.Platforms.Android;

internal static class LocationTrackingBridge
{
    public static Func<LocationReading, Task>? OnReading { get; set; }
}
