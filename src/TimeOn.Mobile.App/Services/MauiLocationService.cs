using Microsoft.Maui.Devices.Sensors;
using TimeOn.Mobile.Core.Interfaces;
using TimeOn.Mobile.Core.Models;

namespace TimeOn.Mobile.App.Services;

public sealed class MauiLocationService : ILocationService
{
    public async Task<LocationSample?> GetCurrentLocationAsync(CancellationToken cancellationToken = default)
    {
        var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
        Location? location = await Geolocation.Default.GetLocationAsync(request, cancellationToken);
        if (location is null)
        {
            return null;
        }

        return new LocationSample(
            location.Latitude,
            location.Longitude,
            Math.Max(0, location.Speed.GetValueOrDefault() * 3.6),
            DateTimeOffset.UtcNow);
    }
}
