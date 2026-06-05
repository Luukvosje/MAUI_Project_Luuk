using TimeOn.Application.Features.WorkSessions.DTOs;
using TimeOn.Domain.Shared;

namespace TimeOn.Application.Features.WorkSessions.Services;

public interface IWorkSessionService
{
    Task<Result<CompleteWorkSessionResponse>> CompleteFromTrackingAsync(CompleteWorkSessionRequest request);
    Task<Result<IReadOnlyList<WorkSessionListItemDto>>> GetAllAsync();
    Task<Result<WorkSessionDetailDto>> GetWorkSessionDetailsAsync(Guid id);
    Task<Result> DeleteAsync(Guid id);
}
