namespace TimeOn.Mobile.Core.Models;

public sealed record LocationSample(
    double Latitude,
    double Longitude,
    double SpeedKmh,
    DateTimeOffset Timestamp);
