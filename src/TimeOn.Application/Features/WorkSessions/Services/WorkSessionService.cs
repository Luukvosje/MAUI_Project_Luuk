using TimeOn.Application.Features.WorkSessions.DTOs;
using TimeOn.Application.Interfaces.Authentication;
using TimeOn.Domain.Entities;
using TimeOn.Domain.Enums;
using TimeOn.Domain.Interfaces;
using TimeOn.Domain.Services;
using TimeOn.Domain.Shared;

using TimeOn.Domain.ValueObjects;

namespace TimeOn.Application.Features.WorkSessions.Services;

public sealed class WorkSessionService : IWorkSessionService
{
    private static readonly IReadOnlyDictionary<Guid, string> EmptyCustomerNames =
        new Dictionary<Guid, string>();

    private readonly IWorkSessionRepository _workSessionRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public WorkSessionService(
        IWorkSessionRepository workSessionRepository,
        ICustomerRepository customerRepository,
        ICurrentUserAccessor currentUserAccessor)
    {
        _workSessionRepository = workSessionRepository;
        _customerRepository = customerRepository;
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
        var customers = await _customerRepository.GetActiveByUserIdAsync(userId.Value);

        var completeResult = session.ClassifyGpsPoints(gpsPoints, customers);
        if (completeResult.IsFailure)
        {
            return Result<CompleteWorkSessionResponse>.Failure(completeResult.Error!);
        }

        var stopResult = session.Stop(request.EndTimeUtc);
        if (stopResult.IsFailure)
        {
            return Result<CompleteWorkSessionResponse>.Failure(stopResult.Error!);
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

        var customers = await _customerRepository.GetActiveByUserIdAsync(userId.Value);
        var customerNames = customers.ToDictionary(customer => customer.Id, customer => customer.Name);

        var segments = session.DrivingSegments
            .Cast<GpsSegment>()
            .Concat(session.StationarySegments)
            .OrderBy(segment => segment.StartUtc)
            .Select(segment => MapSegment(segment, customerNames))
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

    public async Task<Result<WorkSessionSegmentDto>> UpdateSegmentAsync(
        Guid sessionId,
        Guid segmentId,
        UpdateWorkSessionSegmentRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userId = _currentUserAccessor.UserId;
        if (userId is null || userId == Guid.Empty)
        {
            return Result<WorkSessionSegmentDto>.Failure("User is not authenticated.");
        }

        var session = await _workSessionRepository.GetByIdWithDetailsAsync(sessionId, userId.Value);
        if (session is null)
        {
            return Result<WorkSessionSegmentDto>.Failure("Work session not found.");
        }

        var drivingSegment = session.DrivingSegments.FirstOrDefault(segment => segment.Id == segmentId);
        if (drivingSegment is not null)
        {
            var updateResult = UpdateDrivingSegment(drivingSegment, request);
            if (updateResult.IsFailure)
            {
                return Result<WorkSessionSegmentDto>.Failure(updateResult.Error!);
            }

            session.RecalculateTotalDistance();
            _workSessionRepository.Update(session);

            return Result<WorkSessionSegmentDto>.Success(MapSegment(drivingSegment, EmptyCustomerNames));
        }

        var stationarySegment = session.StationarySegments.FirstOrDefault(segment => segment.Id == segmentId);
        if (stationarySegment is not null)
        {
            var updateResult = await UpdateStationarySegmentAsync(stationarySegment, request, userId.Value);
            if (updateResult.IsFailure)
            {
                return Result<WorkSessionSegmentDto>.Failure(updateResult.Error!);
            }

            _workSessionRepository.Update(session);

            var customers = await _customerRepository.GetActiveByUserIdAsync(userId.Value);
            var customerNames = customers.ToDictionary(customer => customer.Id, customer => customer.Name);
            return Result<WorkSessionSegmentDto>.Success(MapSegment(stationarySegment, customerNames));
        }

        return Result<WorkSessionSegmentDto>.Failure("Segment not found.");
    }

    private static Result UpdateDrivingSegment(DrivingSegment segment, UpdateWorkSessionSegmentRequest request)
    {
        var timeResult = segment.UpdateTimes(request.StartUtc, request.EndUtc);
        if (timeResult.IsFailure)
        {
            return timeResult;
        }

        if (!request.DistanceKm.HasValue)
        {
            return Result.Failure("Distance is required for driving segments.");
        }

        return segment.UpdateDistanceKm(request.DistanceKm.Value);
    }

    private async Task<Result> UpdateStationarySegmentAsync(
        StationarySegment segment,
        UpdateWorkSessionSegmentRequest request,
        Guid userId)
    {
        var timeResult = segment.UpdateTimes(request.StartUtc, request.EndUtc);
        if (timeResult.IsFailure)
        {
            return timeResult;
        }

        if (!request.CustomerId.HasValue)
        {
            return segment.UpdateCustomer(null, null);
        }

        var customer = await _customerRepository.GetByIdAsync(request.CustomerId.Value);
        if (customer is null || customer.UserId != userId)
        {
            return Result.Failure("Customer not found.");
        }

        var center = Coordinate.Create(segment.CenterLatitude, segment.CenterLongitude);
        var distance = center.DistanceTo(customer.Location);
        return segment.UpdateCustomer(customer.Id, distance);
    }

    private static WorkSessionSegmentDto MapSegment(
        GpsSegment segment,
        IReadOnlyDictionary<Guid, string> customerNames) => segment switch
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
            stationary.CustomerId.HasValue && customerNames.TryGetValue(stationary.CustomerId.Value, out var customerName)
                ? customerName
                : null,
            stationary.IsCustomerVisit),
        _ => throw new InvalidOperationException($"Unknown segment type: {segment.GetType().Name}")
    };
}