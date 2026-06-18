using TimeOn.Domain.Entities;

namespace TimeOn.Domain.Interfaces;

public interface IWorkSessionRepository
{
    Task<IReadOnlyList<WorkSession>> GetAllByUserIdAsync(Guid userId);
    Task<IReadOnlyList<WorkSession>> GetAllWithSegmentsByUserIdAsync(Guid userId);
    Task<WorkSession?> GetByIdWithDetailsAsync(Guid id, Guid userId);
    Task<WorkSession?> GetActiveByUserIdAsync(Guid userId);
    Task AddAsync(WorkSession workSession);
    void Update(WorkSession workSession);
    Task<bool> DeleteAsync(Guid id, Guid userId);
}
