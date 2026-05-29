using TimeOn.Domain.Constants;
using TimeOn.Domain.Exceptions;
using TimeOn.Domain.Shared;
using TimeOn.Domain.ValueObjects;

namespace TimeOn.Domain.Entities;

public sealed class CustomerVisit : Entity
{
    public Guid WorkSessionId { get; private set; }
    public WorkSession WorkSession { get; private set; } = null!;

    public Guid CustomerId { get; private set; }

    public DateTime ArrivalTimeUtc { get; private set; }

    public DateTime? DepartureTimeUtc { get; private set; }

    public int? DurationMinutes { get; private set; }

    public double DistanceFromCustomerMeters { get; private set; }

    public bool IsActive => DepartureTimeUtc is null;

    private CustomerVisit()
    {
    }

    private CustomerVisit(
        Guid id,
        Guid workSessionId,
        Guid customerId,
        DateTime arrivalTimeUtc,
        double distanceFromCustomerMeters) : base(id)
    {
        WorkSessionId = workSessionId;
        CustomerId = customerId;
        ArrivalTimeUtc = arrivalTimeUtc;
        DistanceFromCustomerMeters = distanceFromCustomerMeters;
    }

    internal static CustomerVisit Start(
        Guid workSessionId,
        Guid customerId,
        DateTime arrivalTimeUtc,
        Distance distanceFromCustomer)
    {
        if (workSessionId == Guid.Empty)
        {
            throw new DomainException("Work session id is required.");
        }

        if (customerId == Guid.Empty)
        {
            throw new DomainException("Customer id is required.");
        }

        TimeRange.EnsureUtc(arrivalTimeUtc, nameof(arrivalTimeUtc));

        if (distanceFromCustomer.Meters > TrackingConstants.CustomerProximityRadiusMeters)
        {
            throw new DomainException(
                $"A customer visit can only be registered within {TrackingConstants.CustomerProximityRadiusMeters} meters of the customer.");
        }

        return new CustomerVisit(
            Guid.NewGuid(),
            workSessionId,
            customerId,
            arrivalTimeUtc,
            distanceFromCustomer.Meters);
    }

    internal Result End(DateTime departureTimeUtc)
    {
        if (!IsActive)
        {
            return Result.Failure("Customer visit is already ended.");
        }

        TimeRange.EnsureUtc(departureTimeUtc, nameof(departureTimeUtc));

        if (departureTimeUtc < ArrivalTimeUtc)
        {
            return Result.Failure("Departure time cannot be earlier than arrival time.");
        }

        DepartureTimeUtc = departureTimeUtc;
        DurationMinutes = TimeRange.Create(ArrivalTimeUtc, departureTimeUtc).DurationMinutes;
        return Result.Success();
    }

    public int CalculateDurationMinutes()
    {
        if (DepartureTimeUtc is null)
        {
            throw new DomainException("Duration can only be calculated for a completed visit.");
        }

        return TimeRange.Create(ArrivalTimeUtc, DepartureTimeUtc.Value).DurationMinutes;
    }
}
