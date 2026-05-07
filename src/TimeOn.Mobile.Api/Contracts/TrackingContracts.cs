namespace TimeOn.Mobile.Api.Contracts;

public sealed record TripDto(
    Guid Id,
    DateTimeOffset StartTime,
    DateTimeOffset? EndTime,
    double DistanceKm,
    string? CustomerName);

public sealed record VisitDto(
    Guid Id,
    string CustomerName,
    DateTimeOffset ArrivedAt,
    DateTimeOffset LeftAt,
    double Latitude,
    double Longitude);

public sealed record CustomerDto(Guid Id, string Name);

public sealed record DayOverviewResponse(
    DateOnly Day,
    double TotalDistanceKm,
    int VisitsCount);
