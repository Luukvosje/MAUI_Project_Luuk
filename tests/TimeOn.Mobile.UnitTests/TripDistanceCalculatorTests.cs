using TimeOn.Mobile.Core.Models;
using TimeOn.Mobile.Core.UseCases;

namespace TimeOn.Mobile.UnitTests;

public sealed class TripDistanceCalculatorTests
{
    [Fact]
    public void CalculateKilometers_WithTwoLocations_ReturnsPositiveDistance()
    {
        var calculator = new TripDistanceCalculator();
        var samples = new List<LocationSample>
        {
            new(52.3676, 4.9041, 40, DateTimeOffset.UtcNow.AddMinutes(-5)),
            new(52.3702, 4.8952, 45, DateTimeOffset.UtcNow),
        };

        double result = calculator.CalculateKilometers(samples);

        Assert.True(result > 0);
    }
}
