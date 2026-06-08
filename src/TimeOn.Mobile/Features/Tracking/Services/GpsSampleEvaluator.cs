using TimeOn.Domain.Constants;
using TimeOn.Domain.Entities;
using TimeOn.Domain.Utilities;
using TimeOn.Mobile.Features.Tracking.Models;
using TimeOn.Mobile.Services;

namespace TimeOn.Mobile.Features.Tracking.Services;

public static class GpsSampleEvaluator
{
#if WINDOWS
    private const double MaxAccuracyMeters = 500;
#else
    private const double MaxAccuracyMeters = TrackingOptions.MaxAccuracyMeters;
#endif

    public static string? GetRejectReason(GpsPoint? lastPoint, LocationReading reading)
    {
        if (lastPoint is not null &&
            reading.HasAccuracy &&
            reading.Accuracy > MaxAccuracyMeters)
        {
            return $"Accuracy {reading.Accuracy:F1}m exceeds max {MaxAccuracyMeters}m";
        }

        if (lastPoint is not null)
        {
            var speedKmh = reading.Speed * 3.6;
            var isStationary = speedKmh < TrackingOptions.StationarySpeedKmh;

            if (!isStationary)
            {
                var distanceMeters = GeoCalculator.HaversineDistance(
                    lastPoint.Location.Latitude,
                    lastPoint.Location.Longitude,
                    reading.Latitude,
                    reading.Longitude);

                if (distanceMeters < TrackingOptions.MinDistanceMeters)
                    return $"Distance {distanceMeters:F1}m below minimum {TrackingOptions.MinDistanceMeters}m";
            }

            var intervalSeconds = speedKmh > TrackingOptions.FastSpeedKmh
                ? TrackingOptions.FastIntervalSeconds
                : isStationary
                    ? TrackingOptions.StationaryIntervalSeconds
                    : TrackingOptions.DefaultIntervalSeconds;
            var elapsed = reading.TimestampUtc - lastPoint.RecordedAtUtc;

            if (elapsed.TotalSeconds <= intervalSeconds)
                return $"Interval not elapsed ({elapsed.TotalSeconds:F0}s < {intervalSeconds}s)";
        }

        return null;
    }

    public static bool ShouldSave(GpsPoint? lastPoint, LocationReading reading) =>
        GetRejectReason(lastPoint, reading) is null;
}