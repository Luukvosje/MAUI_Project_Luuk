using TimeOn.Mobile.Core.Models;

namespace TimeOn.Mobile.Core.UseCases;

public sealed class CustomerSuggestionEngine
{
    public string SuggestCustomer(
        LocationSample currentLocation,
        IReadOnlyList<(string CustomerName, double Latitude, double Longitude)> knownCustomers)
    {
        if (knownCustomers.Count == 0)
        {
            return "Unknown customer";
        }

        var closest = knownCustomers
            .OrderBy(x => DistanceSquared(currentLocation.Latitude, currentLocation.Longitude, x.Latitude, x.Longitude))
            .First();

        return closest.CustomerName;
    }

    private static double DistanceSquared(double lat1, double lon1, double lat2, double lon2)
    {
        double dLat = lat2 - lat1;
        double dLon = lon2 - lon1;
        return (dLat * dLat) + (dLon * dLon);
    }
}
