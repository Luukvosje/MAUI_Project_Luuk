namespace TimeOn.Application.Features.WorkSessions.DTOs;

public sealed record CompleteWorkSessionResponse(
    Guid Id,
    Guid UserId,
    string Status,
    DateTime StartTimeUtc,
    DateTime? EndTimeUtc,
    double TotalDistanceKm,
    int DrivingSegmentCount,
    int StationarySegmentCount);

public sealed record WorkSessionListItemDto(
    Guid Id,
    Guid UserId,
    string Status,
    DateTime StartTimeUtc,
    DateTime? EndTimeUtc,
    double TotalDistanceKm);

public sealed record WorkSessionDetailDto(
    Guid Id,
    Guid UserId,
    string Status,
    DateTime StartTimeUtc,
    DateTime? EndTimeUtc,
    double TotalDistanceKm,
    IReadOnlyList<WorkSessionSegmentDto> Segments);

public sealed record WorkSessionSegmentDto(
    Guid Id,
    string Type,
    DateTime StartUtc,
    DateTime EndUtc,
    int DurationMinutes,
    double? DistanceKm,
    double? CenterLatitude,
    double? CenterLongitude,
    Guid? CustomerId,
    string? CustomerName,
    bool IsCustomerVisit);
