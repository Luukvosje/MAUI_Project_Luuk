using System.Globalization;

namespace TimeOn.Maui.Features.Dashboard.Services;

public static class DashboardMetricsFormatter
{
    private static readonly CultureInfo DutchCulture = new("nl-NL");

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
            return $"{minutes} min";
        }

        var hours = minutes / 60;
        var remainingMinutes = minutes % 60;
        return remainingMinutes == 0
            ? $"{hours} u"
            : $"{hours} u {remainingMinutes} min";
    }

    public static string FormatWeekDayLabel(DateOnly date) =>
        date.ToString("ddd d MMM", DutchCulture);
}
