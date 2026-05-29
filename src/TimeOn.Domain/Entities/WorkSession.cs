using TimeOn.Domain.Constants;
using TimeOn.Domain.Enums;
using TimeOn.Domain.Exceptions;
using TimeOn.Domain.Shared;
using TimeOn.Domain.ValueObjects;

namespace TimeOn.Domain.Entities;


public sealed class WorkSession: Entity
{
    private readonly List<RideSegment> _rideSegments = [];
    private readonly List<CustomerVisit> _customerVisits = [];

    public Guid UserId { get; private set; }

    public DateTime StartTimeUtc { get; private set; }
    public DateTime? EndTimeUtc { get; private set; }

    public double TotalDistanceKm { get; private set; }
    public WorkSessionStatus Status { get; private set; }

    public IReadOnlyCollection<RideSegment> RideSegments => _rideSegments.AsReadOnly();
    public IReadOnlyCollection<CustomerVisit> CustomerVisits => _customerVisits.AsReadOnly();

    private WorkSession()
    {
    }

    private WorkSession(Guid id, Guid userId, DateTime startTimeUtc): base(id)
    {
        UserId = userId;
        StartTimeUtc = startTimeUtc;
        Status = WorkSessionStatus.Active;
        TotalDistanceKm = 0;
    }

    public static WorkSession Start(User user, DateTime startTimeUtc)
    {
        ArgumentNullException.ThrowIfNull(user);
        return Start(user.UserGuid, startTimeUtc);
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

    public Result StartRideSegment(DateTime startTimeUtc)
    {
        if (Status != WorkSessionStatus.Active)
        {
            return Result.Failure("Ride segments can only be started on an active work session.");
        }

        if (GetActiveRideSegment() is not null)
        {
            return Result.Failure("A ride segment is already active.");
        }

        TimeRange.EnsureUtc(startTimeUtc, nameof(startTimeUtc));

        if (startTimeUtc < StartTimeUtc)
        {
            return Result.Failure("Ride segment cannot start before the work session.");
        }

        _rideSegments.Add(RideSegment.Start(Id, startTimeUtc));
        return Result.Success();
    }

    public Result AddCheckpoint(double latitude, double longitude, DateTime recordedAtUtc)
    {
        if (Status != WorkSessionStatus.Active)
        {
            return Result.Failure("Checkpoints can only be recorded on an active work session.");
        }

        var segment = GetActiveRideSegment();
        if (segment is null)
        {
            var startResult = StartRideSegment(recordedAtUtc);
            if (startResult.IsFailure)
            {
                return startResult;
            }

            segment = GetActiveRideSegment();
        }

        var checkpoint = Checkpoint.Create(latitude, longitude, recordedAtUtc);
        return segment!.AddCheckpoint(checkpoint);
    }

    public Result FinishActiveRideSegment(DateTime endTimeUtc, bool clearCheckpoints = true)
    {
        if (Status != WorkSessionStatus.Active)
        {
            return Result.Failure("Ride segments can only be finished on an active work session.");
        }

        var segment = GetActiveRideSegment();
        if (segment is null)
        {
            return Result.Failure("No active ride segment to finish.");
        }

        var finishResult = segment.Finish(endTimeUtc, clearCheckpoints);
        if (finishResult.IsFailure)
        {
            return finishResult;
        }

        RecalculateTotalDistance();
        return Result.Success();
    }

    public Result RegisterCustomerVisit(
        Customer customer,
        Coordinate stopLocation,
        DateTime arrivalTimeUtc)
    {
        ArgumentNullException.ThrowIfNull(customer);
        ArgumentNullException.ThrowIfNull(stopLocation);

        if (Status != WorkSessionStatus.Active)
        {
            return Result.Failure("Customer visits can only be registered on an active work session.");
        }

        if (GetActiveCustomerVisit() is not null)
        {
            return Result.Failure("An active customer visit already exists.");
        }

        if (!customer.IsWithinProximity(stopLocation))
        {
            return Result.Failure(
                $"No customer within {TrackingConstants.CustomerProximityRadiusMeters} meters of the stop location.");
        }

        TimeRange.EnsureUtc(arrivalTimeUtc, nameof(arrivalTimeUtc));

        if (arrivalTimeUtc < StartTimeUtc)
        {
            return Result.Failure("Customer visit cannot start before the work session.");
        }

        var distanceFromCustomer = customer.DistanceTo(stopLocation);
        var visit = CustomerVisit.Start(Id, customer.Id, arrivalTimeUtc, distanceFromCustomer);
        _customerVisits.Add(visit);

        return Result.Success();
    }

    public Result EndActiveCustomerVisit(DateTime departureTimeUtc)
    {
        if (Status != WorkSessionStatus.Active)
        {
            return Result.Failure("Customer visits can only be ended on an active work session.");
        }

        var visit = GetActiveCustomerVisit();
        if (visit is null)
        {
            return Result.Failure("No active customer visit to end.");
        }

        return visit.End(departureTimeUtc);
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

        if (GetActiveCustomerVisit() is not null)
        {
            var endVisitResult = EndActiveCustomerVisit(endTimeUtc);
            if (endVisitResult.IsFailure)
            {
                return endVisitResult;
            }
        }

        if (GetActiveRideSegment() is not null)
        {
            var finishSegmentResult = FinishActiveRideSegment(endTimeUtc, clearCheckpoints);
            if (finishSegmentResult.IsFailure)
            {
                return finishSegmentResult;
            }
        }

        EndTimeUtc = endTimeUtc;
        Status = WorkSessionStatus.Stopped;
        RecalculateTotalDistance();

        return Result.Success();
    }

    public Result Cancel(DateTime cancelledAtUtc)
    {
        if (Status != WorkSessionStatus.Active)
        {
            return Result.Failure("Only an active work session can be cancelled.");
        }

        TimeRange.EnsureUtc(cancelledAtUtc, nameof(cancelledAtUtc));
        EndTimeUtc = cancelledAtUtc;
        Status = WorkSessionStatus.Cancelled;
        return Result.Success();
    }

    public void RecalculateTotalDistance()
    {
        TotalDistanceKm = _rideSegments.Sum(segment => segment.DistanceKm);
    }

    public void ClearAllCheckpoints()
    {
        foreach (var segment in _rideSegments.Where(segment => !segment.IsActive))
        {
            segment.ClearCheckpoints();
        }
    }

    private RideSegment? GetActiveRideSegment() => _rideSegments.FirstOrDefault(segment => segment.IsActive);

    private CustomerVisit? GetActiveCustomerVisit() => _customerVisits.FirstOrDefault(visit => visit.IsActive);
}
