using TimeOn.Domain.ValueObjects;

namespace TimeOn.Domain.Abstractions;

public interface IDistanceCalculator
{
    Distance Calculate(Coordinate from, Coordinate to);
}
