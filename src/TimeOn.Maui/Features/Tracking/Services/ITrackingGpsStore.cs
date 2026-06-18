using TimeOn.Domain.Entities;

namespace TimeOn.Maui.Features.Tracking.Services;

public interface ITrackingGpsStore
{
    Task InitializeAsync();

    Task<ActiveTrackingSession?> GetActiveSessionAsync(Guid userId);

    Task SaveActiveSessionAsync(ActiveTrackingSession session);

    Task ClearActiveSessionAsync(Guid userId);

    Task AddPointAsync(Guid workSessionId, GpsPoint point);

    Task<GpsPoint?> GetLastPointAsync(Guid workSessionId);

    Task<IReadOnlyList<GpsPoint>> GetPointsAsync(Guid workSessionId);

    Task DeletePointsAsync(Guid workSessionId);
}

public sealed record ActiveTrackingSession(Guid Id, Guid UserId, DateTime StartTimeUtc);
