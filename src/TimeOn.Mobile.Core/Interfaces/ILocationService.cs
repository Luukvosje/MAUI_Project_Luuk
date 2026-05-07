using TimeOn.Mobile.Core.Models;

namespace TimeOn.Mobile.Core.Interfaces;

public interface ILocationService
{
    Task<LocationSample?> GetCurrentLocationAsync(CancellationToken cancellationToken = default);
}
