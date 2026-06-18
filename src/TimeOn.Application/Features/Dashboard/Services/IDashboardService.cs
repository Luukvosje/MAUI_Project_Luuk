using TimeOn.Application.Features.Dashboard.DTOs;
using TimeOn.Domain.Shared;

namespace TimeOn.Application.Features.Dashboard.Services;

public interface IDashboardService
{
    Task<Result<DashboardSummaryResponseDto>> GetSummaryAsync(int utcOffsetMinutes);
}
