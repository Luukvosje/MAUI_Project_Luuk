using TimeOn.Domain.Constants;
using TimeOn.Domain.Entities;
using TimeOn.Domain.ValueObjects;

namespace TimeOn.Domain.Services;

public static class CustomerProximityMatcher
{
    public sealed record Match(Guid CustomerId, double DistanceMeters);

    public static Match? FindNearest(Coordinate location, IReadOnlyList<Customer> customers)
    {
        if (customers.Count == 0)
        {
            return null;
        }

        Customer? nearestCustomer = null;
        var nearestDistance = double.MaxValue;

        foreach (var customer in customers)
        {
            if (!customer.IsActive)
            {
                continue;
            }

            var distance = location.DistanceTo(customer.Location);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestCustomer = customer;
            }
        }

        if (nearestCustomer is null || nearestDistance > TrackingConstants.CustomerProximityRadiusMeters)
        {
            return null;
        }

        return new Match(nearestCustomer.Id, nearestDistance);
    }
}
