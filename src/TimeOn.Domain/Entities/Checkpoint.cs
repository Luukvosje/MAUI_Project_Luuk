using TimeOn.Domain.ValueObjects;

namespace TimeOn.Domain.Entities;

public sealed record Checkpoint
{
    public Coordinate Location { get; init; } = null!;

    public DateTime RecordedAtUtc { get; init; }

    private Checkpoint()
    {
    }

    public static Checkpoint Create(double latitude, double longitude, DateTime recordedAtUtc)
    {
        TimeRange.EnsureUtc(recordedAtUtc, nameof(recordedAtUtc));
        return new Checkpoint
        {
            Location = Coordinate.Create(latitude, longitude),
            RecordedAtUtc = recordedAtUtc,
        };
    }
}
