namespace TimeOn.Mobile.Features.Tracking.Models;

public readonly record struct LocationReading(
    double Latitude,
    double Longitude,
    double Accuracy,
    double Speed,
    DateTime TimestampUtc)
{
    /// <summary>
    /// Accuracy is unknown (e.g. platform did not report it). Sample evaluator skips accuracy filtering.
    /// </summary>
    public static double UnknownAccuracy => double.NaN;

    public bool HasAccuracy => !double.IsNaN(Accuracy);
}
