using TimeOn.Application.Features.WorkSessions.DTOs;
using TimeOn.Maui.Features.Tracking.Models;

namespace TimeOn.Maui.Interfaces;

public interface IGpsTrackingService
{
    TrackingState State { get; }

    Guid? CurrentSessionId { get; }

    event EventHandler? StateChanged;

    Task StartAsync();

    Task<StopTrackingResult> StopAsync();

    Task ResumeIfActiveAsync();

    Task<CompleteWorkSessionResponse> SubmitImportedGpsPointsAsync(IReadOnlyList<GpsPointDto> gpsPoints);
}
