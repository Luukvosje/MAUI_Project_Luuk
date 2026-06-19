using System.Text.Json;
using TimeOn.Application.Features.WorkSessions.DTOs;
using TimeOn.Domain.Entities;

namespace TimeOn.UnitTests.Application.WorkSessions;

internal static class GpsTestData
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static readonly Lazy<IReadOnlyList<GpsPointDto>> SamplePointsLazy = new(LoadSamplePoints);
    private static readonly Lazy<IReadOnlyList<GpsPointDto>> EvaluatorSavedPointsLazy =
        new(() => LoadPoints("points-evaluator-saved.json"));

    public static IReadOnlyList<GpsPointDto> SamplePoints => SamplePointsLazy.Value;

    public static IReadOnlyList<GpsPointDto> EvaluatorSavedPoints => EvaluatorSavedPointsLazy.Value;

    public static List<GpsPoint> ToDomainPoints(IReadOnlyList<GpsPointDto> dtos) =>
        dtos.Select(p => GpsPoint.Create(p.Latitude, p.Longitude, p.RecordedAtUtc)).ToList();

    public static CompleteWorkSessionRequest CreateSampleRequest(Guid? sessionId = null)
    {
        var points = SamplePoints;
        var start = points[0].RecordedAtUtc;
        var end = points[^1].RecordedAtUtc;

        return new CompleteWorkSessionRequest(
            sessionId ?? Guid.NewGuid(),
            start,
            end,
            points);
    }

    private static IReadOnlyList<GpsPointDto> LoadSamplePoints() => LoadPoints("points.json");

    private static IReadOnlyList<GpsPointDto> LoadPoints(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "GpsData", fileName);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException(
                $"Test GPS data not found at '{path}'. Ensure {fileName} is copied to the test output (CopyToOutputDirectory).",
                path);
        }

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<IReadOnlyList<GpsPointDto>>(json, JsonOptions)
            ?? throw new InvalidOperationException($"Could not deserialize GPS points from '{path}'.");
    }
}
