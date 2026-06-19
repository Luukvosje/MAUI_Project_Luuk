using System.Globalization;

namespace TimeOn.Maui.Features.Dashboard.Services;

public static class DashboardMetricsFormatter
{
    public static string FormatDistance(double kilometers) =>
        string.Create(CultureInfo.InvariantCulture, $"{kilometers:F1} km");

    public static string FormatCustomerTime(int minutes)
    {
        if (minutes <= 0)
        {
            return "—";
        }

        if (minutes < 60)
        {
            return $"{minutes}m";
        }

        var hours = minutes / 60;
        var remainingMinutes = minutes % 60;
        return remainingMinutes == 0
            ? $"{hours}h"
            : $"{hours}h {remainingMinutes}m";
    }

    public static string FormatWeekDayLabel(DateOnly date) =>
        date.ToString("ddd d MMM", CultureInfo.InvariantCulture);
}
