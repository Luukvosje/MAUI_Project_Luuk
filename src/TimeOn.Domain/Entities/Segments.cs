using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using TimeOn.Domain.Entities;
using TimeOn.Domain.Shared;
using TimeOn.Domain.ValueObjects;

namespace TimeOn.Domain.Entities;

public enum SegmentType
{
    Driving,
    Stationary
}

public abstract class GpsSegment: Entity
{
    public Guid WorkSessionId { get; set; }
    public WorkSession? WorkSession { get; set; }
    public SegmentType Type { get; }
    public DateTime StartUtc { get; }
    public DateTime EndUtc { get; }

    [NotMapped]
    public IReadOnlyList<GpsPoint> Points { get; }

    [NotMapped]
    public TimeSpan Duration => EndUtc - StartUtc;
    [NotMapped]
    public int DurationMinutes => (int)Duration.TotalMinutes;

    protected GpsSegment()
    {
        Points = [];
    }

    protected GpsSegment(SegmentType type, IReadOnlyList<GpsPoint> points)
    {
        if (points == null || points.Count == 0)
            throw new ArgumentException("A segment must contain at least one point.", nameof(points));

        Type = type;
        Points = points;
        StartUtc = points[0].RecordedAtUtc;
        EndUtc = points[^1].RecordedAtUtc;
    }

    public static GpsSegment Create(SegmentType type,IReadOnlyList<GpsPoint> points,Guid? customerId = null,double? distanceFromCustomerMeters = null) 
        => type switch
    {
        SegmentType.Driving => new DrivingSegment(points),
        SegmentType.Stationary => new StationarySegment(points, customerId, distanceFromCustomerMeters),
        _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unknown segment type: {type}")
    };

}

public sealed class DrivingSegment : GpsSegment
{
    public double DistanceMeters { get; }
    public double DistanceKm => DistanceMeters / 1000;

    private DrivingSegment()
    {
    }

    public DrivingSegment(IReadOnlyList<GpsPoint> points)
        : base(SegmentType.Driving, points)
    {
        DistanceMeters = CalculateDistance(points);
    }

    private static double CalculateDistance(IReadOnlyList<GpsPoint> points)
    {
        if (points.Count < 2) return 0;

        var total = 0.0;
        for (var i = 1; i < points.Count; i++)
            total += Distance.Between(points[i - 1].Location, points[i].Location).Meters;

        return total;
    }
}

public sealed class StationarySegment : GpsSegment
{
    public double CenterLatitude { get; }
    public double CenterLongitude { get; }
    public Guid? CustomerId { get; }
    public double? DistanceFromCustomerMeters { get; }

    private StationarySegment()
    {
    }

    public StationarySegment(
        IReadOnlyList<GpsPoint> points,
        Guid? customerId = null,
        double? distanceFromCustomerMeters = null)
        : base(SegmentType.Stationary, points)
    {
        CenterLatitude = points.Average(p => p.Location.Latitude);
        CenterLongitude = points.Average(p => p.Location.Longitude);
        CustomerId = customerId;
        DistanceFromCustomerMeters = distanceFromCustomerMeters;
    }

    public bool IsCustomerVisit => CustomerId.HasValue;
}