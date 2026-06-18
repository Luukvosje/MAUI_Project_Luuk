namespace TimeOn.Application.Features.Dashboard.Services;

public sealed record SummarySegmentInput(
    string Type,
    DateTime StartUtc,
    DateTime EndUtc,
    double? DistanceKm,
    bool IsCustomerVisit);
