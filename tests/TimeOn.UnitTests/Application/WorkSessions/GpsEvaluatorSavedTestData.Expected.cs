namespace TimeOn.UnitTests.Application.WorkSessions;

internal static class GpsEvaluatorSavedTestDataExpected
{
    public const int DrivingSegmentCount = 5;
    public const int StationarySegmentCount = 4;
    public const double TotalDistanceKm = 14.083264;

    public static readonly double[] DrivingSegmentDistancesKm =
    [
        2.698452,
        2.995312,
        2.901777,
        2.728558,
        2.759165,
    ];
}
