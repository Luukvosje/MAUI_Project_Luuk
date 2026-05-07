using TimeOn.Mobile.Core.Models;

namespace TimeOn.Mobile.Core.UseCases;

public sealed class TripDistanceCalculator
{
    public double CalculateKilometers(IReadOnlyList<LocationSample> samples)
    {
        if (samples.Count < 2)
        {
            return 0;
        }

        double distanceKm = 0;
        for (int i = 1; i < samples.Count; i++)
        {
            distanceKm += HaversineDistance(samples[i - 1], samples[i]);
        }

        return Math.Round(distanceKm, 2, MidpointRounding.AwayFromZero);
    }

    private static double HaversineDistance(LocationSample from, LocationSample to)
    {
        const double EarthRadiusKm = 6371;
        double dLat = ToRadians(to.Latitude - from.Latitude);
        double dLon = ToRadians(to.Longitude - from.Longitude);

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(ToRadians(from.Latitude)) * Math.Cos(ToRadians(to.Latitude)) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusKm * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}
