using TimeOn.Application.Features.Dashboard.DTOs;
using TimeOn.Application.Interfaces.Authentication;
using TimeOn.Domain.Entities;
using TimeOn.Domain.Interfaces;
using TimeOn.Domain.Shared;

namespace TimeOn.Application.Features.Dashboard.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly IWorkSessionRepository _workSessionRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public DashboardService(IWorkSessionRepository workSessionRepository, ICurrentUserAccessor currentUserAccessor)
    {
        _workSessionRepository = workSessionRepository;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<Result<DashboardSummaryResponseDto>> GetSummaryAsync(int utcOffsetMinutes)
    {
        var userId = _currentUserAccessor.UserId;
        if (userId is null || userId == Guid.Empty)
        {
            return Result<DashboardSummaryResponseDto>.Failure("User is not authenticated.");
        }

        var sessions = await _workSessionRepository.GetAllWithSegmentsByUserIdAsync(userId.Value);
        var segments = FlattenSegments(sessions);
        var utcOffset = TimeSpan.FromMinutes(utcOffsetMinutes);
        var summary = DashboardSummaryCalculator.Build(segments, utcOffset, DateTime.UtcNow);

        return Result<DashboardSummaryResponseDto>.Success(summary);
    }

    private static IReadOnlyList<SummarySegmentInput> FlattenSegments(IReadOnlyList<WorkSession> sessions)
    {
        var segments = new List<SummarySegmentInput>();

        foreach (var session in sessions)
        {
            foreach (var driving in session.DrivingSegments)
            {
                segments.Add(new SummarySegmentInput(
                    nameof(SegmentType.Driving),
                    driving.StartUtc,
                    driving.EndUtc,
                    driving.DistanceKm,
                    IsCustomerVisit: false));
            }

            foreach (var stationary in session.StationarySegments)
            {
                segments.Add(new SummarySegmentInput(
                    nameof(SegmentType.Stationary),
                    stationary.StartUtc,
                    stationary.EndUtc,
                    DistanceKm: null,
                    stationary.IsCustomerVisit));
            }
        }

        return segments;
    }
}
