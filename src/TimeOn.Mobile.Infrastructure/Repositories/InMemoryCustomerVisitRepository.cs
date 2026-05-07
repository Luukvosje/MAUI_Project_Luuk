using TimeOn.Mobile.Core.Interfaces;
using TimeOn.Mobile.Core.Models;

namespace TimeOn.Mobile.Infrastructure.Repositories;

public sealed class InMemoryCustomerVisitRepository : ICustomerVisitRepository
{
    private readonly List<CustomerVisit> visits = [];

    public Task<IReadOnlyList<CustomerVisit>> GetVisitsForDayAsync(DateOnly day, CancellationToken cancellationToken = default)
    {
        var result = visits
            .Where(x => DateOnly.FromDateTime(x.ArrivedAt.LocalDateTime) == day)
            .OrderBy(x => x.ArrivedAt)
            .ToList();

        return Task.FromResult<IReadOnlyList<CustomerVisit>>(result);
    }

    public Task SaveVisitAsync(CustomerVisit visit, CancellationToken cancellationToken = default)
    {
        visits.RemoveAll(x => x.Id == visit.Id);
        visits.Add(visit);
        return Task.CompletedTask;
    }
}
