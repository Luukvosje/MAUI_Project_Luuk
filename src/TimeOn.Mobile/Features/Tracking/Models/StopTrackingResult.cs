namespace TimeOn.Mobile.Features.Tracking.Models;

public sealed record StopTrackingResult(bool SubmittedToApi, int GpsPointCount);
