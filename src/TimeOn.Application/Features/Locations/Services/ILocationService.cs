using TimeOn.Domain.Shared;
using TimeOn.Application.Features.Locations.DTOs;

namespace TimeOn.Application.Features.Locations.Services;

public interface ILocationService
{
    Task<Result<IReadOnlyList<LocationPointDto>>> GetByTripIdAsync(Guid tripId);
}
