using System.Globalization;
using TimeOn.Application.Features.Dashboard.DTOs;
using TimeOn.Domain.Shared;
using TimeOn.Maui.Interfaces;

namespace TimeOn.Maui.Features.Dashboard.Services;

public interface IDashboardSummaryService
{
    Task<Result<DashboardSummaryResponseDto>> GetSummaryAsync();
}
