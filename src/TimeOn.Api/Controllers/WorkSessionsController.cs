using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeOn.Infrastructure.Persistence;
using System.Linq.Expressions;

namespace TimeOn.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class WorkSessionsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public WorkSessionsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var sessions = await _dbContext.WorkSessions
            .AsNoTracking()
            .OrderByDescending(session => session.StartTimeUtc)
            .Select(MapToResponse())
            .ToListAsync();

        return Ok(sessions);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var session = await _dbContext.WorkSessions
            .AsNoTracking()
            .Where(item => item.Id == id)
            .Select(MapToResponse())
            .FirstOrDefaultAsync();

        return session is null ? NotFound() : Ok(session);
    }

    [HttpGet("by-user/{userId:guid}")]
    public async Task<IActionResult> GetByUserId(Guid userId)
    {
        var sessions = await _dbContext.WorkSessions
            .AsNoTracking()
            .Where(session => session.UserId == userId)
            .OrderByDescending(session => session.StartTimeUtc)
            .Select(MapToResponse())
            .ToListAsync();

        return Ok(sessions);
    }

    private static Expression<Func<TimeOn.Domain.Entities.WorkSession, WorkSessionResponse>> MapToResponse() =>
        session => new WorkSessionResponse(
            session.Id,
            session.UserId,
            session.Status.ToString(),
            session.StartTimeUtc,
            session.EndTimeUtc,
            session.TotalDistanceKm);

    private sealed record WorkSessionResponse(
        Guid Id,
        Guid UserId,
        string Status,
        DateTime StartTimeUtc,
        DateTime? EndTimeUtc,
        double TotalDistanceKm);
}
