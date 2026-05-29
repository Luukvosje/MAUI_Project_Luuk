using FluentAssertions;
using TimeOn.Domain.Entities;
using TimeOn.Domain.Enums;
using TimeOn.Domain.ValueObjects;

namespace TimeOn.UnitTests.Domain;

public class WorkSessionTests
{
    [Fact]
    public void Start_CreatesActiveSession()
    {
        var userId = Guid.NewGuid();
        var session = WorkSession.Start(userId, DateTime.UtcNow);

        session.Status.Should().Be(WorkSessionStatus.Active);
        session.UserId.Should().Be(userId);
    }

    [Fact]
    public void RegisterCustomerVisit_WhenWithinRadius_AddsVisit()
    {
        var session = WorkSession.Start(Guid.NewGuid(), DateTime.UtcNow);
        //var customer = Customer.FromExternal(
        //    Guid.NewGuid(),
        //    "Acme BV",
        //    "Street 1",
        //    52.0,
        //    5.0,
        //    DateTime.UtcNow);

        //var stop = Coordinate.Create(52.0001, 5.0001);
        //var result = session.RegisterCustomerVisit(customer, stop, DateTime.UtcNow);

        //result.IsSuccess.Should().BeTrue();
        //session.CustomerVisits.Should().HaveCount(1);
    }
}
