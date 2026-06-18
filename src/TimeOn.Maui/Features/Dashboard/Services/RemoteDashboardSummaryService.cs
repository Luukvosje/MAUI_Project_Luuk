using TimeOn.Application.Features.Dashboard.DTOs;
using TimeOn.Domain.Shared;
using TimeOn.Maui.Interfaces;

namespace TimeOn.Maui.Features.Dashboard.Services;

public sealed class RemoteDashboardSummaryService : IDashboardSummaryService
{
    private const string SummaryEndpoint = "api/dashboard/summary";
    private readonly IApiService _apiService;

    public RemoteDashboardSummaryService(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<Result<DashboardSummaryResponseDto>> GetSummaryAsync()
    {
        try
        {
            var utcOffsetMinutes = (int)TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).TotalMinutes;
            var response = await _apiService.GetAsync<DashboardSummaryResponseDto>(
                $"{SummaryEndpoint}?utcOffsetMinutes={utcOffsetMinutes}");

            return response is null
                ? Result<DashboardSummaryResponseDto>.Failure(_apiService.LastError ?? "Kon dashboardoverzicht niet laden.")
                : Result<DashboardSummaryResponseDto>.Success(response);
        }
        catch (Exception)
        {
            return Result<DashboardSummaryResponseDto>.Failure(_apiService.LastError ?? "Kon dashboardoverzicht niet laden.");
        }
    }
}
