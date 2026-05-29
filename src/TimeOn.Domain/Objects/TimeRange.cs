using TimeOn.Domain.Exceptions;
namespace TimeOn.Domain.ValueObjects;
public sealed class TimeRange
{
    public DateTime StartUtc { get; init; }
    public DateTime EndUtc { get; init; }
    private TimeRange() { }
    public static TimeRange Create(DateTime startUtc, DateTime endUtc)
    {
        EnsureUtc(startUtc, nameof(startUtc));
        EnsureUtc(endUtc, nameof(endUtc));
        if (endUtc < startUtc) { throw new DomainException("End time must be greater than or equal to start time."); }
        return new TimeRange { StartUtc = startUtc, EndUtc = endUtc };
    }
    public TimeSpan Duration => EndUtc - StartUtc;
    public int DurationMinutes => (int)Math.Ceiling(Duration.TotalMinutes);
    public bool Contains(DateTime instantUtc) => instantUtc >= StartUtc && instantUtc <= EndUtc;
    public static void EnsureUtc(DateTime value, string parameterName)
    {
        if (value.Kind != DateTimeKind.Utc) { throw new DomainException($"{parameterName} must be specified in UTC."); }
    }
}
