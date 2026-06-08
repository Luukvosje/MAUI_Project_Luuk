namespace TimeOn.Application.Features.WorkSessions.DTOs;

public sealed record CompleteWorkSessionRequest(
    Guid SessionId,
    DateTime StartTimeUtc,
    DateTime EndTimeUtc,
    IReadOnlyList<GpsPointDto> GpsPoints);

public sealed record GpsPointDto(
    double Latitude,
    double Longitude,
    DateTime RecordedAtUtc);

public sealed record UpdateWorkSessionSegmentRequest(
    DateTime StartUtc,
    DateTime EndUtc,
    double? DistanceKm,
    Guid? CustomerId);
