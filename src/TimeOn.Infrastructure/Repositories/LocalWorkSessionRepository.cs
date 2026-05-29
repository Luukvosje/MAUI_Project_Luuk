using Microsoft.EntityFrameworkCore;
using TimeOn.Domain.Entities;
using TimeOn.Domain.Enums;
using TimeOn.Domain.RepositoryInterfaces;
using TimeOn.Infrastructure.Persistence;

namespace TimeOn.Infrastructure.Repositories;

public sealed class LocalWorkSessionRepository : IWorkSessionRepository, ILocalWorkSessionRepository
{
    private readonly LocalDbContext _context;

    public LocalWorkSessionRepository(LocalDbContext context)
    {
        _context = context;
    }

    public async Task<WorkSession?> GetByIdAsync(Guid id, bool useLocal = false)
    {
        return await _context.WorkSessions
            .FirstOrDefaultAsync(session => session.Id == id);
    }

    public Task<WorkSession?> GetByIdWithDetailsAsync(Guid id)
    {
        return GetByIdWithDetailsAsync(id, useLocal: true);
    }

    public async Task<WorkSession?> GetByIdWithDetailsAsync(Guid id, bool useLocal = false)
    {
        return await _context.WorkSessions
            .Include(session => session.RideSegments)
            .Include(session => session.CustomerVisits)
            .FirstOrDefaultAsync(session => session.Id == id);
    }

    public async Task<WorkSession?> GetActiveByUserIdAsync(Guid userId, bool useLocal = false)
    {
        return await _context.WorkSessions
            .Include(session => session.RideSegments)
            .Include(session => session.CustomerVisits)
            .FirstOrDefaultAsync(
                session => session.UserId == userId && session.Status == WorkSessionStatus.Active);
    }

    public Task<WorkSession?> GetActiveByUserIdAsync(Guid userId)
    {
        return GetActiveByUserIdAsync(userId, useLocal: true);
    }

    public async Task<IReadOnlyList<WorkSession>> GetByUserIdAsync(Guid userId, bool useLocal = false)
    {
        return await _context.WorkSessions
            .AsNoTracking()
            .Where(session => session.UserId == userId)
            .OrderByDescending(session => session.StartTimeUtc)
            .ToListAsync();
    }

    public Task<IReadOnlyList<WorkSession>> GetByUserIdAsync(Guid userId)
    {
        return GetByUserIdAsync(userId, useLocal: true);
    }

    public async Task AddAsync(WorkSession workSession, bool useLocal = false)
    {
        await _context.WorkSessions.AddAsync(workSession);
        await _context.SaveChangesAsync();
    }

    public Task AddAsync(WorkSession workSession)
    {
        return AddAsync(workSession, useLocal: true);
    }

    public void Update(WorkSession workSession, bool useLocal = false)
    {
        _context.WorkSessions.Update(workSession);
        _context.SaveChanges();
    }

    public void Update(WorkSession workSession)
    {
        Update(workSession, useLocal: true);
    }

    public void Remove(WorkSession workSession)
    {
        _context.WorkSessions.Remove(workSession);
        _context.SaveChanges();
    }
}
