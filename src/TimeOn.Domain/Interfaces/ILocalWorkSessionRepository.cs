using TimeOn.Domain.Entities;

namespace TimeOn.Domain.Interfaces;

public interface ILocalWorkSessionRepository
{
    Task<WorkSession?> GetByIdWithDetailsAsync(Guid id);
    Task<WorkSession?> GetActiveByUserIdAsync(Guid userId);
    Task<IReadOnlyList<WorkSession>> GetByUserIdAsync(Guid userId);
    Task AddAsync(WorkSession workSession);
    void Update(WorkSession workSession);
    void Remove(WorkSession workSession);
}
