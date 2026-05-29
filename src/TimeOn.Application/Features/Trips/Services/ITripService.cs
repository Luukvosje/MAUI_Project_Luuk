using TimeOn.Domain.Shared;
using TimeOn.Application.Features.Trips.DTOs;

namespace TimeOn.Application.Features.Trips.Services;

public interface ITripService
{
    Task<Result<TripDto>> GetByIdAsync(Guid id);
    Task<Result<IReadOnlyList<TripDto>>> GetByUserIdAsync(Guid userId);
}
