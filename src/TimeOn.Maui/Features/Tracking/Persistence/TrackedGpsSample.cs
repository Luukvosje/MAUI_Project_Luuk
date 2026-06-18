using SQLite;

namespace TimeOn.Maui.Features.Tracking.Persistence;

[Table("TrackedGpsSamples")]
public sealed class TrackedGpsSample
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public string WorkSessionId { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public DateTime RecordedAtUtc { get; set; }
}
