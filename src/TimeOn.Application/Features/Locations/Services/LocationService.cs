using TimeOn.Domain.Shared;
using TimeOn.Application.Features.Locations.DTOs;
using TimeOn.Domain.RepositoryInterfaces;

namespace TimeOn.Application.Features.Locations.Services;

public sealed class LocationService : ILocationService
{
    private readonly ILocalWorkSessionRepository _localWorkSessionRepository;

    public LocationService(ILocalWorkSessionRepository localWorkSessionRepository)
    {
        _localWorkSessionRepository = localWorkSessionRepository;
    }

    public Task<Result<IReadOnlyList<LocationPointDto>>> GetByTripIdAsync(Guid tripId)
    {
        _ = _localWorkSessionRepository;
        _ = tripId;
        
        return Task.FromResult(Result<IReadOnlyList<LocationPointDto>>.Failure("Not implemented."));
    }
}
