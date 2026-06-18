namespace TimeOn.Maui.Features.Tracking.Models;

public sealed record StopTrackingResult(bool SubmittedToApi, int GpsPointCount);
