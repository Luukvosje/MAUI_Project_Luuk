using TimeOn.Mobile.Core.Models;
using TimeOn.Mobile.Core.UseCases;

namespace TimeOn.Mobile.UnitTests;

public sealed class StandstillDetectorTests
{
    [Fact]
    public void IsStandstill_WithSlowSamplesForLongEnough_ReturnsTrue()
    {
        var detector = new StandstillDetector();
        var start = DateTimeOffset.UtcNow.AddMinutes(-4);
        var samples = new List<LocationSample>
        {
            new(52.3676, 4.9041, 0.5, start),
            new(52.3676, 4.9042, 0.7, start.AddMinutes(2)),
            new(52.3677, 4.9042, 0.9, start.AddMinutes(4)),
        };

        bool result = detector.IsStandstill(samples);

        Assert.True(result);
    }
}
