namespace TimeOn.Domain.Constants;


public static class TrackingConstants
{
    public const double CheckpointIntervalMeters = 500;

    public const double CustomerProximityRadiusMeters = 500;

    public static readonly TimeSpan MinimumStopDurationMinutes = TimeSpan.FromMinutes(2);
    public const int MaxStationaryDistanceMeters = 150;
    public const int MinRidingSpeedKph = 10;

    public const double EarthRadiusMeters = 6_371_000;

    public const int MaxCustomerNameLength = 200;

    public const int MaxCustomerAddressLength = 500;
    public const int MaxCustomerContactEmailLength = 320;

}
public static class TrackingOptions
{
    public const int DefaultIntervalSeconds = 15;
    public const int FastIntervalSeconds = 5;
    public const int StationaryIntervalSeconds = 30;
    public const double FastSpeedKmh = 10;
    public const double StationarySpeedKmh = 2;  
    public const double MinDistanceMeters = 25;
    public const double MaxAccuracyMeters = 50;
}