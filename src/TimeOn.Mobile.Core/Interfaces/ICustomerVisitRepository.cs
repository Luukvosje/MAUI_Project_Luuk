using TimeOn.Mobile.Core.Models;

namespace TimeOn.Mobile.Core.Interfaces;

public interface ICustomerVisitRepository
{
    Task<IReadOnlyList<CustomerVisit>> GetVisitsForDayAsync(DateOnly day, CancellationToken cancellationToken = default);

    Task SaveVisitAsync(CustomerVisit visit, CancellationToken cancellationToken = default);
}
