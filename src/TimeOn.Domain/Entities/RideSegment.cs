using TimeOn.Domain.Exceptions;
using TimeOn.Domain.Shared;
using TimeOn.Domain.ValueObjects;

namespace TimeOn.Domain.Entities;

public sealed class RideSegment : Entity
{
    private readonly List<Checkpoint> _checkpoints = [];
    public double DistanceMeters { get; private set; }

    public Guid WorkSessionId { get; private set; }
    public WorkSession WorkSession { get; private set; } = null!;

    public DateTime StartTimeUtc { get; private set; }

    public DateTime? EndTimeUtc { get; private set; }

    public double DistanceKm => DistanceMeters / 1000;

    public IReadOnlyCollection<Checkpoint> Checkpoints => _checkpoints.AsReadOnly();

    public bool IsActive => EndTimeUtc is null;

    private RideSegment()
    {
    }

    private RideSegment(Guid id, Guid workSessionId, DateTime startTimeUtc) : base(id)
    {
        WorkSessionId = workSessionId;
        StartTimeUtc = startTimeUtc;
    }

    internal static RideSegment Start(Guid workSessionId, DateTime startTimeUtc)
    {
        if (workSessionId == Guid.Empty)
        {
            throw new DomainException("Work session id is required.");
        }

        TimeRange.EnsureUtc(startTimeUtc, nameof(startTimeUtc));
        return new RideSegment(Guid.NewGuid(), workSessionId, startTimeUtc);
    }

    internal Result AddCheckpoint(Checkpoint checkpoint)
    {
        ArgumentNullException.ThrowIfNull(checkpoint);

        if (!IsActive)
        {
            return Result.Failure("Checkpoints can only be added to an active ride segment.");
        }

        if (_checkpoints.Count > 0)
        {
            var last = _checkpoints[^1];
            if (checkpoint.RecordedAtUtc < last.RecordedAtUtc)
            {
                return Result.Failure("Checkpoint timestamps must be chronologically ordered.");
            }
        }

        _checkpoints.Add(checkpoint);
        RecalculateDistance();
        return Result.Success();
    }

    internal Result Finish(DateTime endTimeUtc, bool clearCheckpoints)
    {
        if (!IsActive)
        {
            return Result.Failure("Ride segment is already finished.");
        }

        TimeRange.EnsureUtc(endTimeUtc, nameof(endTimeUtc));

        if (endTimeUtc < StartTimeUtc)
        {
            return Result.Failure("Ride segment end time cannot be earlier than the start time.");
        }

        EndTimeUtc = endTimeUtc;
        RecalculateDistance();

        if (clearCheckpoints)
        {
            ClearCheckpoints();
        }

        return Result.Success();
    }

    public void RecalculateDistance()
    {
        if (_checkpoints.Count < 2)
        {
            DistanceMeters = 0;
            return;
        }

        var totalMeters = 0.0;
        for (var index = 1; index < _checkpoints.Count; index++)
        {
            totalMeters += Distance.Between(
                _checkpoints[index - 1].Location,
                _checkpoints[index].Location).Meters;
        }

        DistanceMeters = totalMeters;
    }
    public void ClearCheckpoints() => _checkpoints.Clear();
}
