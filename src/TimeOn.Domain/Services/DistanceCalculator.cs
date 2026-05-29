using TimeOn.Domain.Abstractions;
using TimeOn.Domain.ValueObjects;

namespace TimeOn.Domain.Services;

public sealed class DistanceCalculator : IDistanceCalculator
{
    public Distance Calculate(Coordinate from, Coordinate to) => Distance.Between(from, to);
}
