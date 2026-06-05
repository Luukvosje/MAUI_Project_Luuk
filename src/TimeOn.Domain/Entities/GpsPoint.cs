using TimeOn.Domain.ValueObjects;

namespace TimeOn.Domain.Entities;

public sealed record GpsPoint
{
    public Coordinate Location { get; init; } = null!;

    public DateTime RecordedAtUtc { get; init; }

    private GpsPoint()
    {
    }

    public static GpsPoint Create(double latitude, double longitude, DateTime recordedAtUtc)
    {
        TimeRange.EnsureUtc(recordedAtUtc, nameof(recordedAtUtc));
        return new GpsPoint
        {
            Location = Coordinate.Create(latitude, longitude),
            RecordedAtUtc = recordedAtUtc,
        };
    }
}
