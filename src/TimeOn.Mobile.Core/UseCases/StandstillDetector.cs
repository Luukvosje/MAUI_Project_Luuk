using TimeOn.Mobile.Core.Models;

namespace TimeOn.Mobile.Core.UseCases;

public sealed class StandstillDetector
{
    private readonly double speedThresholdKmh;
    private readonly TimeSpan minimumDuration;

    public StandstillDetector(double speedThresholdKmh = 2, int minimumMinutes = 3)
    {
        this.speedThresholdKmh = speedThresholdKmh;
        minimumDuration = TimeSpan.FromMinutes(minimumMinutes);
    }

    public bool IsStandstill(IReadOnlyList<LocationSample> samples)
    {
        if (samples.Count < 2)
        {
            return false;
        }

        var windowStart = samples[0].Timestamp;
        var windowEnd = samples[^1].Timestamp;
        bool allBelowThreshold = samples.All(x => x.SpeedKmh <= speedThresholdKmh);

        return allBelowThreshold && (windowEnd - windowStart) >= minimumDuration;
    }
}
