namespace TimeOn.Domain.Constants;


public static class TrackingConstants
{
    public const double CheckpointIntervalMeters = 500;

    public const double CustomerProximityRadiusMeters = 500;

    public static readonly TimeSpan MinimumStopDuration = TimeSpan.FromMinutes(2);
    public const int MinRidingSpeedKph = 10;

    public const double EarthRadiusMeters = 6_371_000;

    public const int MaxCustomerNameLength = 200;

    public const int MaxCustomerAddressLength = 500;
    public const int MaxCustomerContactEmailLength = 320;

}
