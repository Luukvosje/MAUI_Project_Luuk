using System.Text.Json;
using TimeOn.Application.Features.WorkSessions.DTOs;

namespace TimeOn.Mobile.Features.Tracking.Services;

public static class GpsPointsClipboardParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static IReadOnlyList<GpsPointDto> Parse(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new InvalidOperationException("GPS JSON is empty.");
        }

        var trimmed = json.Trim();
        List<GpsPointDto>? points;

        try
        {
            points = JsonSerializer.Deserialize<List<GpsPointDto>>(trimmed, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                "Input is not a valid JSON array of GPS points.",
                ex);
        }

        if (points is null || points.Count == 0)
        {
            throw new InvalidOperationException("No GPS points found in JSON.");
        }

        var invalid = points.FirstOrDefault(point =>
            point.Latitude is < -90 or > 90 ||
            point.Longitude is < -180 or > 180 ||
            point.RecordedAtUtc == default);

        if (invalid is not null)
        {
            throw new InvalidOperationException(
                "Each GPS point needs latitude, longitude, and recordedAtUtc.");
        }

        return points
            .OrderBy(point => point.RecordedAtUtc)
            .ToList();
    }
}
