namespace TimeOn.Mobile.Core.Models;

public sealed class CustomerVisit
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string CustomerName { get; init; } = string.Empty;

    public DateTimeOffset ArrivedAt { get; init; }

    public DateTimeOffset LeftAt { get; init; }

    public LocationSample Location { get; init; } = new(0, 0, 0, DateTimeOffset.UtcNow);
}
