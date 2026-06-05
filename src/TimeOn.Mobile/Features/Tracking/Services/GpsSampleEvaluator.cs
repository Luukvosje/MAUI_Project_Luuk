using TimeOn.Domain.Constants;
using TimeOn.Domain.Entities;
using TimeOn.Mobile.Features.Tracking.Models;

namespace TimeOn.Mobile.Features.Tracking.Services;

public static class GpsSampleEvaluator
{
#if WINDOWS
    private const double MaxAccuracyMeters = 500;
#else
    private const double MaxAccuracyMeters = TrackingOptions.MaxAccuracyMeters;
#endif
    public static string? GetRejectReason(GpsPoint? lastPoint, LocationReading reading) =>
        ShouldSave(lastPoint, reading)
            ? null
            : reading.HasAccuracy
                ? $"Accuracy {reading.Accuracy:F1}m exceeds max {MaxAccuracyMeters}m"
                : "Rejected by sample evaluator";

    public static bool ShouldSave(GpsPoint? lastPoint, LocationReading reading)
    {
        if (reading.HasAccuracy && reading.Accuracy > MaxAccuracyMeters)
        {
            return false;
        }

        if (lastPoint is null)
        {
            return true;
        }

        var speedKmh = reading.Speed * 3.6;
        var intervalSeconds = speedKmh > TrackingOptions.FastSpeedKmh
            ? TrackingOptions.FastIntervalSeconds
            : TrackingOptions.DefaultIntervalSeconds;
        var elapsed = reading.TimestampUtc - lastPoint.RecordedAtUtc;

        return elapsed.TotalSeconds > intervalSeconds;
    }
};