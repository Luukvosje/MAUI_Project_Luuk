using Microsoft.EntityFrameworkCore;
using TimeOn.Domain.Entities;
using TimeOn.Domain.Enums;
using TimeOn.Domain.Interfaces;
using TimeOn.Infrastructure.Persistence;

namespace TimeOn.Infrastructure.Repositories;

public sealed class WorkSessionRepository : IWorkSessionRepository
{
    private readonly AppDbContext _remoteContext;

    public WorkSessionRepository(AppDbContext remoteContext)
    {
        _remoteContext = remoteContext;
    }

    public async Task<IReadOnlyList<WorkSession>> GetAllByUserIdAsync(Guid userId)
    {
        return await _remoteContext.WorkSessions
            .AsNoTracking()
            .Where(session => session.UserId == userId)
            .OrderByDescending(session => session.StartTimeUtc)
            .ToListAsync();
    }

    public async Task<WorkSession?> GetByIdWithDetailsAsync(Guid id, Guid userId)
    {
        return await _remoteContext.WorkSessions
            .Where(session => session.Id == id && session.UserId == userId)
            .Include(session => session.DrivingSegments)
            .Include(session => session.StationarySegments)
            .FirstOrDefaultAsync(session => session.Id == id);
    }

    public async Task<WorkSession?> GetActiveByUserIdAsync(Guid userId)
    {
        return await _remoteContext.WorkSessions
            .Include(session => session.DrivingSegments)
            .Include(session => session.StationarySegments)
            .FirstOrDefaultAsync(
                session => session.UserId == userId && session.Status == WorkSessionStatus.Active);
    }

    public async Task AddAsync(WorkSession workSession)
    {
        await _remoteContext.WorkSessions.AddAsync(workSession);
        await _remoteContext.SaveChangesAsync();
    }

    public void Update(WorkSession workSession)
    {

        _remoteContext.WorkSessions.Update(workSession);
        _remoteContext.SaveChanges();
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var session = await _remoteContext.WorkSessions
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (session is null)
        {
            return false;
        }

        _remoteContext.WorkSessions.Remove(session);
        await _remoteContext.SaveChangesAsync();
        return true;
    }
}
