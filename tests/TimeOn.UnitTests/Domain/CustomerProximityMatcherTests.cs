using FluentAssertions;
using TimeOn.Domain.Constants;
using TimeOn.Domain.Entities;
using TimeOn.Domain.Services;
using TimeOn.Domain.ValueObjects;

namespace TimeOn.UnitTests.Domain;

public class CustomerProximityMatcherTests
{
    [Fact]
    public void FindNearest_ReturnsClosestActiveCustomerWithinRadius()
    {
        var location = Coordinate.Create(52.0, 5.0);
        var nearCustomer = CreateCustomer("Near", 52.0001, 5.0001);
        var farCustomer = CreateCustomer("Far", 52.01, 5.01);

        var match = CustomerProximityMatcher.FindNearest(
            location,
            [nearCustomer, farCustomer]);

        match.Should().NotBeNull();
        match!.CustomerId.Should().Be(nearCustomer.Id);
        match.DistanceMeters.Should().BeLessThan(TrackingConstants.CustomerProximityRadiusMeters);
    }

    [Fact]
    public void FindNearest_ReturnsNullWhenNoCustomerWithinRadius()
    {
        var location = Coordinate.Create(52.0, 5.0);
        var farCustomer = CreateCustomer("Far", 52.1, 5.1);

        var match = CustomerProximityMatcher.FindNearest(location, [farCustomer]);

        match.Should().BeNull();
    }

    [Fact]
    public void FindNearest_IgnoresInactiveCustomers()
    {
        var location = Coordinate.Create(52.0, 5.0);
        var inactiveCustomer = CreateCustomer("Inactive", 52.0001, 5.0001, isActive: false);

        var match = CustomerProximityMatcher.FindNearest(location, [inactiveCustomer]);

        match.Should().BeNull();
    }

    private static Customer CreateCustomer(
        string name,
        double latitude,
        double longitude,
        bool isActive = true)
    {
        return Customer.Create(
            Guid.NewGuid(),
            name,
            Guid.NewGuid(),
            $"{name.ToLowerInvariant()}@test.com",
            "Test address",
            isActive,
            Coordinate.Create(latitude, longitude));
    }
}
