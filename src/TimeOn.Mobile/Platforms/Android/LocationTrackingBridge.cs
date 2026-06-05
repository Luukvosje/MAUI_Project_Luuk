using TimeOn.Mobile.Features.Tracking.Models;

namespace TimeOn.Mobile.Platforms.Android;

internal static class LocationTrackingBridge
{
    public static Func<LocationReading, Task>? OnReading { get; set; }
}
