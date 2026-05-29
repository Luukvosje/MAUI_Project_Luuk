using TimeOn.Domain.Constants;
using TimeOn.Domain.Exceptions;
namespace TimeOn.Domain.ValueObjects;
public sealed class Distance : IComparable<Distance>
{
    public double Meters { get; init; }
    private Distance() { }
    public static Distance FromMeters(double meters)
    {
        if (meters < 0) { throw new DomainException("Distance cannot be negative."); }
        if (double.IsNaN(meters) || double.IsInfinity(meters)) { throw new DomainException("Distance must be a finite number."); }
        return new Distance { Meters = meters };
    }
    public static Distance FromKilometers(double kilometers) => FromMeters(kilometers * 1000);
    public double ToKilometers() => Meters / 1000;
    public static Distance operator +(Distance left, Distance right) => FromMeters(left.Meters + right.Meters);
    public static Distance Between(Coordinate from, Coordinate to)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);
        var lat1 = DegreesToRadians(from.Latitude);
        var lat2 = DegreesToRadians(to.Latitude);
        var deltaLat = DegreesToRadians(to.Latitude - from.Latitude);
        var deltaLon = DegreesToRadians(to.Longitude - from.Longitude);
        var haversine = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
        var centralAngle = 2 * Math.Atan2(Math.Sqrt(haversine), Math.Sqrt(1 - haversine));
        return FromMeters(TrackingConstants.EarthRadiusMeters * centralAngle);
    }
    public int CompareTo(Distance? other) => other is null ? 1 : Meters.CompareTo(other.Meters);
    public override string ToString() => $"{Meters:F1} m";
    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180;
}
