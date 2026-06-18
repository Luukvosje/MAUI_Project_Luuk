using TimeOn.Application.Features.Dashboard.DTOs;

namespace TimeOn.Application.Features.Dashboard.Services;

public static class DashboardSummaryCalculator
{
    public static DashboardSummaryResponseDto Build(
        IReadOnlyList<SummarySegmentInput> segments,
        TimeSpan utcOffset,
        DateTime utcNow)
    {
        var referenceToday = DateOnly.FromDateTime(utcNow.Add(utcOffset));
        var day = BuildDaySummary(segments, utcOffset, referenceToday);
        var week = BuildWeekSummary(segments, utcOffset, referenceToday);
        var hasAnyActivity = segments.Any(HasSegmentActivity);

        return new DashboardSummaryResponseDto(day, week, hasAnyActivity);
    }

    public static DashboardDaySummaryDto BuildDaySummary(
        IReadOnlyList<SummarySegmentInput> segments,
        TimeSpan utcOffset,
        DateOnly referenceToday)
    {
        var metrics = CalculateDayMetrics(segments, utcOffset, referenceToday);
        if (metrics.TotalDistanceKm > 0 || metrics.CustomerMinutes > 0)
        {
            return CreateDaySummary(referenceToday, metrics, isFallback: false, referenceToday);
        }

        var latestActivityDate = FindLatestActivityDate(segments, utcOffset);
        if (latestActivityDate is null || latestActivityDate == referenceToday)
        {
            return CreateDaySummary(referenceToday, metrics, isFallback: false, referenceToday);
        }

        var fallbackMetrics = CalculateDayMetrics(segments, utcOffset, latestActivityDate.Value);
        return CreateDaySummary(latestActivityDate.Value, fallbackMetrics, isFallback: true, referenceToday);
    }

    public static DashboardWeekSummaryDto BuildWeekSummary(
        IReadOnlyList<SummarySegmentInput> segments,
        TimeSpan utcOffset,
        DateOnly referenceToday)
    {
        var currentWeekStart = GetWeekStartMonday(referenceToday);
        var currentWeek = CreateWeekSummary(segments, utcOffset, currentWeekStart, isFallback: false, referenceToday);
        if (currentWeek.HasActivity)
        {
            return currentWeek;
        }

        var latestActivityDate = FindLatestActivityDate(segments, utcOffset);
        if (latestActivityDate is null)
        {
            return currentWeek;
        }

        var fallbackWeekStart = GetWeekStartMonday(latestActivityDate.Value);
        if (fallbackWeekStart == currentWeekStart)
        {
            return currentWeek;
        }

        return CreateWeekSummary(segments, utcOffset, fallbackWeekStart, isFallback: true, referenceToday);
    }

    public static (double TotalDistanceKm, int CustomerMinutes) CalculateDayMetrics(
        IReadOnlyList<SummarySegmentInput> segments,
        TimeSpan utcOffset,
        DateOnly day)
    {
        var periodStart = day.ToDateTime(TimeOnly.MinValue);
        var periodEnd = day.ToDateTime(TimeOnly.MaxValue);

        double totalDistanceKm = 0;
        var customerMinutes = 0;

        foreach (var segment in segments)
        {
            if (!TryGetOverlapFraction(segment, utcOffset, periodStart, periodEnd, out var fraction))
            {
                continue;
            }

            if (IsDriving(segment) && segment.DistanceKm is > 0)
            {
                totalDistanceKm += segment.DistanceKm.Value * fraction;
            }

            if (IsStationary(segment) && segment.IsCustomerVisit)
            {
                var durationMinutes = GetDurationMinutes(segment);
                customerMinutes += (int)Math.Round(durationMinutes * fraction, MidpointRounding.AwayFromZero);
            }
        }

        return (totalDistanceKm, customerMinutes);
    }

    private static DashboardDaySummaryDto CreateDaySummary(
        DateOnly date,
        (double TotalDistanceKm, int CustomerMinutes) metrics,
        bool isFallback,
        DateOnly referenceToday) =>
        new(
            date,
            metrics.TotalDistanceKm,
            metrics.CustomerMinutes,
            isFallback,
            FormatDayLabel(date, referenceToday, isFallback));

    private static DashboardWeekSummaryDto CreateWeekSummary(
        IReadOnlyList<SummarySegmentInput> segments,
        TimeSpan utcOffset,
        DateOnly weekStart,
        bool isFallback,
        DateOnly referenceToday)
    {
        var weekEnd = weekStart.AddDays(6);
        var days = new List<DashboardDayBreakdownDto>(7);
        double totalDistanceKm = 0;
        var totalCustomerMinutes = 0;

        for (var offset = 0; offset < 7; offset++)
        {
            var day = weekStart.AddDays(offset);
            var metrics = CalculateDayMetrics(segments, utcOffset, day);
            totalDistanceKm += metrics.TotalDistanceKm;
            totalCustomerMinutes += metrics.CustomerMinutes;

            days.Add(new DashboardDayBreakdownDto(
                day,
                metrics.TotalDistanceKm,
                metrics.CustomerMinutes,
                metrics.TotalDistanceKm > 0 || metrics.CustomerMinutes > 0));
        }

        return new DashboardWeekSummaryDto(
            weekStart,
            weekEnd,
            totalDistanceKm,
            totalCustomerMinutes,
            isFallback,
            FormatWeekLabel(weekStart, weekEnd, referenceToday, isFallback),
            days);
    }

    private static DateOnly? FindLatestActivityDate(
        IReadOnlyList<SummarySegmentInput> segments,
        TimeSpan utcOffset)
    {
        DateOnly? latest = null;

        foreach (var segment in segments)
        {
            if (!HasSegmentActivity(segment))
            {
                continue;
            }

            var startDate = DateOnly.FromDateTime(ToLocal(segment.StartUtc, utcOffset));
            var endDate = DateOnly.FromDateTime(ToLocal(segment.EndUtc, utcOffset));

            for (var day = startDate; day <= endDate; day = day.AddDays(1))
            {
                var metrics = CalculateDayMetrics(segments, utcOffset, day);
                if (metrics.TotalDistanceKm <= 0 && metrics.CustomerMinutes <= 0)
                {
                    continue;
                }

                if (latest is null || day > latest)
                {
                    latest = day;
                }
            }
        }

        return latest;
    }

    private static bool TryGetOverlapFraction(
        SummarySegmentInput segment,
        TimeSpan utcOffset,
        DateTime periodStart,
        DateTime periodEnd,
        out double fraction)
    {
        fraction = 0;

        var startLocal = ToLocal(segment.StartUtc, utcOffset);
        var endLocal = ToLocal(segment.EndUtc, utcOffset);
        if (endLocal <= startLocal)
        {
            return false;
        }

        var overlapStart = startLocal > periodStart ? startLocal : periodStart;
        var overlapEnd = endLocal < periodEnd ? endLocal : periodEnd;
        if (overlapStart >= overlapEnd)
        {
            return false;
        }

        fraction = (overlapEnd - overlapStart).TotalSeconds / (endLocal - startLocal).TotalSeconds;
        return fraction > 0;
    }

    private static bool HasSegmentActivity(SummarySegmentInput segment) =>
        (IsDriving(segment) && segment.DistanceKm is > 0)
        || (IsStationary(segment) && segment.IsCustomerVisit);

    private static bool IsDriving(SummarySegmentInput segment) =>
        string.Equals(segment.Type, "Driving", StringComparison.OrdinalIgnoreCase);

    private static bool IsStationary(SummarySegmentInput segment) =>
        string.Equals(segment.Type, "Stationary", StringComparison.OrdinalIgnoreCase);

    private static double GetDurationMinutes(SummarySegmentInput segment) =>
        (segment.EndUtc - segment.StartUtc).TotalMinutes;

    private static DateTime ToLocal(DateTime utc, TimeSpan utcOffset) =>
        DateTime.SpecifyKind(utc, DateTimeKind.Utc).Add(utcOffset);

    private static DateOnly GetWeekStartMonday(DateOnly date)
    {
        var daysFromMonday = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return date.AddDays(-daysFromMonday);
    }

    private static string FormatDayLabel(DateOnly date, DateOnly referenceToday, bool isFallback)
    {
        if (!isFallback && date == referenceToday)
        {
            return "Today";
        }

        if (isFallback)
        {
            return $"No activity today — showing {date:ddd d MMM}";
        }

        return date.ToString("ddd d MMM");
    }

    private static string FormatWeekLabel(
        DateOnly weekStart,
        DateOnly weekEnd,
        DateOnly referenceToday,
        bool isFallback)
    {
        var currentWeekStart = GetWeekStartMonday(referenceToday);
        var rangeLabel = $"{weekStart:d MMM} – {weekEnd:d MMM}";

        if (!isFallback && weekStart == currentWeekStart)
        {
            return $"This week ({rangeLabel})";
        }

        if (isFallback)
        {
            return $"No activity this week — showing {rangeLabel}";
        }

        return rangeLabel;
    }
}
