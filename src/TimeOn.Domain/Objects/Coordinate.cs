using TimeOn.Domain.Constants;using TimeOn.Domain.Exceptions;namespace TimeOn.Domain.ValueObjects;
public sealed class Coordinate{    public double Latitude { get; init; }    public double Longitude { get; init; }    public static Coordinate Create(double latitude, double longitude)    {
        if (latitude is < -90 or > 90)        {            throw new DomainException("Latitude must be between -90 and 90 degrees.");        }
        if (longitude is < -180 or > 180)        {            throw new DomainException("Longitude must be between -180 and 180 degrees.");        }
        return new Coordinate { Latitude = latitude, Longitude = longitude };    }

    public double DistanceTo(Coordinate other)
    {
        const double R = TrackingConstants.EarthRadiusMeters;

        double phi1 = ToRadians(Latitude);
        double phi2 = ToRadians(other.Latitude);
        double dPhi = ToRadians(other.Latitude - Latitude);
        double dLambda = ToRadians(other.Longitude - Longitude);

        double a = Math.Sin(dPhi / 2) * Math.Sin(dPhi / 2)
                 + Math.Cos(phi1) * Math.Cos(phi2)
                 * Math.Sin(dLambda / 2) * Math.Sin(dLambda / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;    public override string ToString() => $"{Latitude:F6}, {Longitude:F6}";}