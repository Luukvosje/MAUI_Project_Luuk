using TimeOn.Domain.Shared;
using TimeOn.Application.Features.Trips.DTOs;
using TimeOn.Domain.Interfaces;

namespace TimeOn.Application.Features.Trips.Services;

public sealed class TripService : ITripService
{
    private readonly IWorkSessionRepository _workSessionRepository;

    public TripService(IWorkSessionRepository workSessionRepository)
    {
        _workSessionRepository = workSessionRepository;
    }

    public Task<Result<TripDto>> GetByIdAsync(Guid id)
    {
        _ = _workSessionRepository;
        _ = id;
        
        return Task.FromResult(Result<TripDto>.Failure("Not implemented."));
    }

    public Task<Result<IReadOnlyList<TripDto>>> GetAllByUserId(Guid userId)
    {
        _ = _workSessionRepository;
        _ = userId;

        
        
        return Task.FromResult(Result<IReadOnlyList<TripDto>>.Failure("Not implemented."));
    }
}
