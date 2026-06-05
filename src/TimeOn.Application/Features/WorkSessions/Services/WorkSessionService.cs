using TimeOn.Application.Features.WorkSessions.DTOs;
using TimeOn.Application.Interfaces.Authentication;
using TimeOn.Domain.Entities;
using TimeOn.Domain.Enums;
using TimeOn.Domain.Interfaces;
using TimeOn.Domain.Services;
using TimeOn.Domain.Shared;

namespace TimeOn.Application.Features.WorkSessions.Services;

public sealed class WorkSessionService : IWorkSessionService
{
    private readonly IWorkSessionRepository _workSessionRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public WorkSessionService(
        IWorkSessionRepository workSessionRepository,
        ICurrentUserAccessor currentUserAccessor)
    {
        _workSessionRepository = workSessionRepository;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<Result<CompleteWorkSessionResponse>> CompleteFromTrackingAsync(CompleteWorkSessionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userId = _currentUserAccessor.UserId;
        if (userId is null || userId == Guid.Empty)
        {
            return Result<CompleteWorkSessionResponse>.Failure("User is not authenticated.");
        }

        if (request.GpsPoints.Count == 0)
        {
            return Result<CompleteWorkSessionResponse>.Failure("At least one GPS point is required.");
        }

        var gpsPoints = request.GpsPoints
            .Select(point => GpsPoint.Create(point.Latitude, point.Longitude, point.RecordedAtUtc))
            .ToList();

        var session = WorkSession.RestoreActive(request.SessionId, userId.Value, request.StartTimeUtc);

        var completeResult = session.ClassifyGpsPoints(gpsPoints);
        if (completeResult.IsFailure)
        {
            return Result<CompleteWorkSessionResponse>.Failure(completeResult.Error!);
        }

        await _workSessionRepository.AddAsync(session);

        return Result<CompleteWorkSessionResponse>.Success(MapToResponse(session));
    }


    private static CompleteWorkSessionResponse MapToResponse(WorkSession session) => new(session.Id, session.UserId, session.Status.ToString(), session.StartTimeUtc, session.EndTimeUtc, session.TotalDistanceKm, session.DrivingSegments.Count, session.StationarySegments.Count);

    public async Task<Result<IReadOnlyList<WorkSessionListItemDto>>> GetAllAsync()
    {
        var userId = _currentUserAccessor.UserId;
        if (userId is null || userId == Guid.Empty)
        {
            return Result<IReadOnlyList<WorkSessionListItemDto>>.Failure("User is not authenticated.");
        }

        var sessions = await _workSessionRepository.GetAllByUserIdAsync(userId.Value);
        var items = sessions
            .Select(session => new WorkSessionListItemDto(
                session.Id,
                session.UserId,
                session.Status.ToString(),
                session.StartTimeUtc,
                session.EndTimeUtc,
                session.TotalDistanceKm))
            .ToList();

        return Result<IReadOnlyList<WorkSessionListItemDto>>.Success(items);
    }

    public async Task<Result<WorkSessionDetailDto>> GetWorkSessionDetailsAsync(Guid id)
    {
        var userId = _currentUserAccessor.UserId;
        if (userId is null || userId == Guid.Empty)
        {
            return Result<WorkSessionDetailDto>.Failure("User is not authenticated.");
        }

        var session = await _workSessionRepository.GetByIdWithDetailsAsync(id, userId.Value);

        if (session is null)
        {
            return Result<WorkSessionDetailDto>.Failure("Work session not found.");
        }

        var segments = session.DrivingSegments
            .Cast<GpsSegment>()
            .Concat(session.StationarySegments)
            .OrderBy(segment => segment.StartUtc)
            .Select(MapSegment)
            .ToList();

        return Result<WorkSessionDetailDto>.Success(new WorkSessionDetailDto(
            session.Id,
            session.UserId,
            session.Status.ToString(),
            session.StartTimeUtc,
            session.EndTimeUtc,
            session.TotalDistanceKm,
            segments));
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var userId = _currentUserAccessor.UserId;
        if (userId is null || userId == Guid.Empty)
        {
            return Result.Failure("User is not authenticated.");
        }

        var deleted = await _workSessionRepository.DeleteAsync(id, userId.Value);
        return deleted
            ? Result.Success()
            : Result.Failure("Work session not found.");
    }

    private static WorkSessionSegmentDto MapSegment(GpsSegment segment) => segment switch
    {
        DrivingSegment driving => new WorkSessionSegmentDto(
            driving.Id,
            nameof(SegmentType.Driving),
            driving.StartUtc,
            driving.EndUtc,
            driving.DurationMinutes,
            driving.DistanceKm,
            null,
            null,
            null,
            false),
        StationarySegment stationary => new WorkSessionSegmentDto(
            stationary.Id,
            nameof(SegmentType.Stationary),
            stationary.StartUtc,
            stationary.EndUtc,
            stationary.DurationMinutes,
            null,
            stationary.CenterLatitude,
            stationary.CenterLongitude,
            stationary.CustomerId,
            stationary.IsCustomerVisit),
        _ => throw new InvalidOperationException($"Unknown segment type: {segment.GetType().Name}")
    };
}