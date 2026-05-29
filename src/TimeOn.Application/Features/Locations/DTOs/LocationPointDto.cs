namespace TimeOn.Application.Features.Locations.DTOs;

public sealed record LocationPointDto(
    Guid Id,
    Guid TripId,
    double Latitude,
    double Longitude,
    DateTime RecordedAt,
    double? AccuracyMeters);
