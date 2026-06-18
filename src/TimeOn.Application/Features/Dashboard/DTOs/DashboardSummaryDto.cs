namespace TimeOn.Application.Features.Dashboard.DTOs;

public sealed record DashboardSummaryResponseDto(
    DashboardDaySummaryDto Day,
    DashboardWeekSummaryDto Week,
    bool HasAnyActivity);

public sealed record DashboardDaySummaryDto(
    DateOnly Date,
    double TotalDistanceKm,
    int CustomerMinutes,
    bool IsFallback,
    string PeriodLabel)
{
    public bool HasActivity => TotalDistanceKm > 0 || CustomerMinutes > 0;
}

public sealed record DashboardWeekSummaryDto(
    DateOnly WeekStart,
    DateOnly WeekEnd,
    double TotalDistanceKm,
    int CustomerMinutes,
    bool IsFallback,
    string PeriodLabel,
    IReadOnlyList<DashboardDayBreakdownDto> Days)
{
    public bool HasActivity => TotalDistanceKm > 0 || CustomerMinutes > 0;
}

public sealed record DashboardDayBreakdownDto(
    DateOnly Date,
    double TotalDistanceKm,
    int CustomerMinutes,
    bool HasActivity);
