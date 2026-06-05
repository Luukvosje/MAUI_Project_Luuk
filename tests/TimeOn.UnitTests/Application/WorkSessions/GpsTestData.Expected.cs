namespace TimeOn.UnitTests.Application.WorkSessions;

internal static class GpsTestDataExpected
{
    public const int DrivingSegmentCount = 5;
    public const int StationarySegmentCount = 4;
    public const double TotalDistanceKm = 21.009740;

    public static readonly double[] DrivingSegmentDistancesKm =
    [
        2.859103,
        3.084458,
        2.988298,
        2.817421,
        9.260460,
    ];
}
