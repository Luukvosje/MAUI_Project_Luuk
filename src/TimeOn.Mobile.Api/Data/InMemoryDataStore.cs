using TimeOn.Mobile.Api.Contracts;

namespace TimeOn.Mobile.Api.Data;

public sealed class InMemoryDataStore
{
    private readonly List<TripDto> trips = [];
    private readonly List<VisitDto> visits = [];
    private readonly List<CustomerDto> customers =
    [
        new CustomerDto(Guid.NewGuid(), "Time On Rotterdam"),
        new CustomerDto(Guid.NewGuid(), "Time On Utrecht"),
        new CustomerDto(Guid.NewGuid(), "Time On Eindhoven")
    ];

    public IReadOnlyCollection<TripDto> Trips => trips;

    public IReadOnlyCollection<VisitDto> Visits => visits;

    public IReadOnlyCollection<CustomerDto> Customers => customers;

    public void AddTrip(TripDto trip) => trips.Add(trip);

    public void AddVisit(VisitDto visit) => visits.Add(visit);
}
