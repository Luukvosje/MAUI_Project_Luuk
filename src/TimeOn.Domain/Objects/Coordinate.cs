using TimeOn.Domain.Exceptions;namespace TimeOn.Domain.ValueObjects;
public sealed class Coordinate{    public double Latitude { get; init; }    public double Longitude { get; init; }    public static Coordinate Create(double latitude, double longitude)    {
        if (latitude is < -90 or > 90)        {            throw new DomainException("Latitude must be between -90 and 90 degrees.");        }
        if (longitude is < -180 or > 180)        {            throw new DomainException("Longitude must be between -180 and 180 degrees.");        }
        return new Coordinate { Latitude = latitude, Longitude = longitude };    }    public override string ToString() => $"{Latitude:F6}, {Longitude:F6}";}