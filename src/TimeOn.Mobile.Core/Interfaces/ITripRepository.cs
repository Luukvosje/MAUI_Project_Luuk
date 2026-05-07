using TimeOn.Mobile.Core.Models;

namespace TimeOn.Mobile.Core.Interfaces;

public interface ITripRepository
{
    Task<IReadOnlyList<Trip>> GetTripsForDayAsync(DateOnly day, CancellationToken cancellationToken = default);

    Task SaveTripAsync(Trip trip, CancellationToken cancellationToken = default);
}
