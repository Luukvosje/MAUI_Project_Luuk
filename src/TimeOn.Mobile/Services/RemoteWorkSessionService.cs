using TimeOn.Application.Features.WorkSessions.DTOs;
using TimeOn.Application.Features.WorkSessions.Services;
using TimeOn.Domain.Shared;
using TimeOn.Mobile.Interfaces;

namespace TimeOn.Mobile.Services;

public sealed class RemoteWorkSessionService : IWorkSessionService
{
    private const string WorkSessionsEndpoint = "api/worksessions";
    private readonly IApiService _apiService;

    public RemoteWorkSessionService(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<Result<CompleteWorkSessionResponse>> CompleteFromTrackingAsync(CompleteWorkSessionRequest request)
    {
        try
        {
            var response = await _apiService.PostAsync<CompleteWorkSessionRequest, CompleteWorkSessionResponse>(
                $"{WorkSessionsEndpoint}/complete",
                request);
            return response is null
                ? Result<CompleteWorkSessionResponse>.Failure(_apiService.LastError ?? "Could not complete work session.")
                : Result<CompleteWorkSessionResponse>.Success(response);
        }
        catch (Exception)
        {
            return Result<CompleteWorkSessionResponse>.Failure(_apiService.LastError ?? "Could not complete work session.");
        }
    }

    public async Task<Result<IReadOnlyList<WorkSessionListItemDto>>> GetAllAsync()
    {
        try
        {
            var sessions = await _apiService.GetAsync<IReadOnlyList<WorkSessionListItemDto>>(WorkSessionsEndpoint);
            return sessions is null
                ? Result<IReadOnlyList<WorkSessionListItemDto>>.Failure(_apiService.LastError ?? "Could not load work sessions.")
                : Result<IReadOnlyList<WorkSessionListItemDto>>.Success(sessions);
        }
        catch (Exception)
        {
            return Result<IReadOnlyList<WorkSessionListItemDto>>.Failure(_apiService.LastError ?? "Could not load work sessions.");
        }
    }

    public async Task<Result<WorkSessionDetailDto>> GetWorkSessionDetailsAsync(Guid id)
    {
        try
        {
            var session = await _apiService.GetAsync<WorkSessionDetailDto>($"{WorkSessionsEndpoint}/{id}");
            return session is null
                ? Result<WorkSessionDetailDto>.Failure(_apiService.LastError ?? "Could not load work session.")
                : Result<WorkSessionDetailDto>.Success(session);
        }
        catch (Exception)
        {
            return Result<WorkSessionDetailDto>.Failure(_apiService.LastError ?? "Could not load work session.");
        }
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        try
        {
            await _apiService.DeleteAsync($"{WorkSessionsEndpoint}/{id}");
            return _apiService.LastError is null
                ? Result.Success()
                : Result.Failure(_apiService.LastError);
        }
        catch (Exception)
        {
            return Result.Failure(_apiService.LastError ?? "Could not delete work session.");
        }
    }

    public async Task<Result<WorkSessionSegmentDto>> UpdateSegmentAsync(
        Guid sessionId,
        Guid segmentId,
        UpdateWorkSessionSegmentRequest request)
    {
        try
        {
            var segment = await _apiService.PutAsync<UpdateWorkSessionSegmentRequest, WorkSessionSegmentDto>(
                $"{WorkSessionsEndpoint}/{sessionId}/segments/{segmentId}",
                request);

            return segment is null
                ? Result<WorkSessionSegmentDto>.Failure(_apiService.LastError ?? "Could not update segment.")
                : Result<WorkSessionSegmentDto>.Success(segment);
        }
        catch (Exception)
        {
            return Result<WorkSessionSegmentDto>.Failure(_apiService.LastError ?? "Could not update segment.");
        }
    }
}
