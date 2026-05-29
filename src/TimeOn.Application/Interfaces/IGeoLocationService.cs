using TimeOn.Domain.Shared;
using TimeOn.Domain.ValueObjects;

namespace TimeOn.Application.Interfaces
{
    public interface IGeoLocationService
    {
        Task<Result<Coordinate>> AdressToCoordinate(string address);
    }
}
