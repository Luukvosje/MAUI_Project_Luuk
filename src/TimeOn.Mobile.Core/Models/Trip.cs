namespace TimeOn.Mobile.Core.Models;

public sealed class Trip
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public DateTimeOffset StartTime { get; init; }

    public DateTimeOffset? EndTime { get; set; }

    public double DistanceKm { get; set; }

    public string? CustomerName { get; set; }
}
