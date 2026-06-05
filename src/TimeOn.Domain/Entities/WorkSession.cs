using TimeOn.Domain.Constants;
using TimeOn.Domain.Enums;
using TimeOn.Domain.Exceptions;
using TimeOn.Domain.Services;
using TimeOn.Domain.Shared;
using TimeOn.Domain.ValueObjects;

namespace TimeOn.Domain.Entities;


public sealed class WorkSession: Entity
{
    private readonly List<DrivingSegment> _drivingSegments = [];
    private readonly List<StationarySegment> _stationarySegments = [];

    public Guid UserId { get; private set; }

    public DateTime StartTimeUtc { get; private set; }
    public DateTime? EndTimeUtc { get; private set; }

    public double TotalDistanceKm { get; private set; }
    public WorkSessionStatus Status { get; private set; }

    public IReadOnlyCollection<DrivingSegment> DrivingSegments => _drivingSegments.AsReadOnly();
    public IReadOnlyCollection<StationarySegment> StationarySegments => _stationarySegments.AsReadOnly();

    private WorkSession(Guid id, Guid userId, DateTime startTimeUtc): base(id)
    {
        UserId = userId;
        StartTimeUtc = startTimeUtc;
        Status = WorkSessionStatus.Active;
        TotalDistanceKm = 0;
    }

    public static WorkSession Start(Guid userId, DateTime startTimeUtc)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("User id is required.");
        }

        TimeRange.EnsureUtc(startTimeUtc, nameof(startTimeUtc));

        var session = new WorkSession(Guid.NewGuid(), userId, startTimeUtc);
        return session;
    }

    public static WorkSession RestoreActive(Guid id, Guid userId, DateTime startTimeUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Work session id is required.");
        }

        if (userId == Guid.Empty)
        {
            throw new DomainException("User id is required.");
        }

        TimeRange.EnsureUtc(startTimeUtc, nameof(startTimeUtc));

        return new WorkSession(id, userId, startTimeUtc);
    }

    public static WorkSession RestoreCompleted(
        Guid id,
        Guid userId,
        DateTime startTimeUtc,
        DateTime endTimeUtc,
        WorkSessionStatus status,
        double totalDistanceKm)
    {
        var session = RestoreActive(id, userId, startTimeUtc);
        session.EndTimeUtc = endTimeUtc;
        session.Status = status;
        session.TotalDistanceKm = totalDistanceKm;
        return session;
    }

    public Result ClassifyGpsPoints(IReadOnlyList<GpsPoint> gpsPoints)
    {
        var segments = new GpsClassifier().Classify(gpsPoints);
        return ApplyClassifiedSegments(segments);
    }

    public Result ApplyClassifiedSegments(IReadOnlyList<GpsSegment> segments)
    {
        if (Status != WorkSessionStatus.Active)
        {
            return Result.Failure("Segments can only be applied to an active work session.");
        }

        ClearAllCheckpoints();

        foreach (var segment in segments)
        {
            segment.WorkSessionId = Id;

            switch (segment)
            {
                case DrivingSegment driving:
                    _drivingSegments.Add(driving);
                    break;
                case StationarySegment stationary:
                    _stationarySegments.Add(stationary);
                    break;
            }
        }

        RecalculateTotalDistance();
        return Result.Success();
    }

    public Result Stop(DateTime endTimeUtc, bool clearCheckpoints = true)
    {
        if (Status != WorkSessionStatus.Active)
        {
            return Result.Failure("Only an active work session can be stopped.");
        }

        TimeRange.EnsureUtc(endTimeUtc, nameof(endTimeUtc));

        if (endTimeUtc < StartTimeUtc)
        {
            return Result.Failure("Work session end time cannot be earlier than the start time.");
        }

        EndTimeUtc = endTimeUtc;
        Status = WorkSessionStatus.Stopped;
        RecalculateTotalDistance();

        return Result.Success();
    }

    public void RecalculateTotalDistance()
    {
        TotalDistanceKm = _drivingSegments.Sum(segment => segment.DistanceKm);
    }

    public void ClearAllCheckpoints()
    {
        _drivingSegments.Clear();
        _stationarySegments.Clear();
    }
}
