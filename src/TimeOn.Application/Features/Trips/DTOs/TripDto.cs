using TimeOn.Domain.Enums;

namespace TimeOn.Application.Features.Trips.DTOs;

public sealed record TripDto(
    Guid Id,
    Guid UserId,
    WorkSessionStatus Status,
    DateTime StartTimeUtc,
    DateTime? EndTimeUtc,
    double TotalDistanceKm);
