using TimeOn.Domain.Entities;

namespace TimeOn.Domain.RepositoryInterfaces;

/// <summary>
/// Persistence contract for <see cref="WorkSession"/> aggregate roots (API / server store).
/// </summary>
public interface IWorkSessionRepository
{
    Task<WorkSession?> GetByIdAsync(Guid id, bool useLocal = false);
    Task<WorkSession?> GetByIdWithDetailsAsync(Guid id, bool useLocal = false);
    Task<WorkSession?> GetActiveByUserIdAsync(Guid userId, bool useLocal = false);
    Task<IReadOnlyList<WorkSession>> GetByUserIdAsync(Guid userId, bool useLocal = false);
    Task AddAsync(WorkSession workSession, bool useLocal = false);
    void Update(WorkSession workSession, bool useLocal = false);
}
