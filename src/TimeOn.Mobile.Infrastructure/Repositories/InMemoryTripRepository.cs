using TimeOn.Mobile.Core.Interfaces;
using TimeOn.Mobile.Core.Models;

namespace TimeOn.Mobile.Infrastructure.Repositories;

public sealed class InMemoryTripRepository : ITripRepository
{
    private readonly List<Trip> trips = [];

    public Task<IReadOnlyList<Trip>> GetTripsForDayAsync(DateOnly day, CancellationToken cancellationToken = default)
    {
        var result = trips
            .Where(x => DateOnly.FromDateTime(x.StartTime.LocalDateTime) == day)
            .OrderBy(x => x.StartTime)
            .ToList();

        return Task.FromResult<IReadOnlyList<Trip>>(result);
    }

    public Task SaveTripAsync(Trip trip, CancellationToken cancellationToken = default)
    {
        trips.RemoveAll(x => x.Id == trip.Id);
        trips.Add(trip);
        return Task.CompletedTask;
    }
}
