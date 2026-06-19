using FluentAssertions;
using TimeOn.Domain.Entities;
using TimeOn.Domain.Enums;
using TimeOn.Domain.Services;
using TimeOn.Domain.ValueObjects;
using TimeOn.UnitTests.Application.WorkSessions;

namespace TimeOn.UnitTests.Domain;

public class GpsClassifierTests
{
    [Fact]
    public void Classify_EvaluatorSavedTrace_ProducesExpectedDrivingAndStationarySegments()
    {
        var points = GpsTestData.ToDomainPoints(GpsTestData.EvaluatorSavedPoints);

        var segments = new GpsClassifier().Classify(points);

        var drivingSegments = segments
            .Where(segment => segment.Type == SegmentType.Driving)
            .Cast<DrivingSegment>()
            .ToList();
        var stationarySegments = segments
            .Where(segment => segment.Type == SegmentType.Stationary)
            .ToList();

        drivingSegments.Should().HaveCount(GpsEvaluatorSavedTestDataExpected.DrivingSegmentCount);
        stationarySegments.Should().HaveCount(GpsEvaluatorSavedTestDataExpected.StationarySegmentCount);

        var drivingDistancesKm = drivingSegments.Select(segment => segment.DistanceKm).ToArray();
        drivingDistancesKm.Should().HaveCount(GpsEvaluatorSavedTestDataExpected.DrivingSegmentDistancesKm.Length);

        for (var i = 0; i < drivingDistancesKm.Length; i++)
        {
            drivingDistancesKm[i].Should().BeApproximately(
                GpsEvaluatorSavedTestDataExpected.DrivingSegmentDistancesKm[i],
                precision: 0.001,
                because: $"driving segment {i + 1} distance should match saved trace");
        }

        drivingDistancesKm.Sum().Should().BeApproximately(
            GpsEvaluatorSavedTestDataExpected.TotalDistanceKm,
            precision: 0.001);
    }

    [Fact]
    public void Classify_WithCustomersAtStopLocations_LinksStationarySegmentsToNearestCustomer()
    {
        var points = GpsTestData.ToDomainPoints(GpsTestData.EvaluatorSavedPoints);
        var customers = new[]
        {
            CreateCustomerAt(51.5936, 5.5005),
            CreateCustomerAt(51.5723, 5.5291),
            CreateCustomerAt(51.5512, 5.5558),
            CreateCustomerAt(51.5310, 5.5800),
        };

        var segments = new GpsClassifier().Classify(points, customers);
        var visits = segments
            .OfType<StationarySegment>()
            .Where(segment => segment.IsCustomerVisit)
            .ToList();

        visits.Should().HaveCount(GpsEvaluatorSavedTestDataExpected.StationarySegmentCount);
        visits.Should().OnlyContain(
            visit => visit.DistanceFromCustomerMeters.HasValue
                && visit.DistanceFromCustomerMeters.Value <= 500,
            because: "each garden stop should match a customer within proximity radius");
        visits.Select(visit => visit.CustomerId).Should().BeEquivalentTo(customers.Select(customer => customer.Id));
    }

    private static Customer CreateCustomerAt(double latitude, double longitude) =>
        Customer.Create(
            Guid.NewGuid(),
            $"Customer at {latitude:F4}",
            Guid.NewGuid(),
            "customer@test.com",
            "Test address",
            isActive: true,
            Coordinate.Create(latitude, longitude));
}
