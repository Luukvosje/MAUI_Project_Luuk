using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeOn.Application.Interfaces.Authentication;
using TimeOn.Application.Features.WorkSessions.DTOs;
using TimeOn.Application.Features.WorkSessions.Services;
using TimeOn.Infrastructure.Persistence;
using System.Linq.Expressions;

namespace TimeOn.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class WorkSessionsController(
    AppDbContext dbContext,
    IWorkSessionService workSessionService,
    ICurrentUserAccessor currentUserAccessor) : ControllerBase
{
    [HttpPost("complete")]
    public async Task<IActionResult> Complete([FromBody] CompleteWorkSessionRequest request)
    {
        var result = await workSessionService.CompleteFromTrackingAsync(request);
        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = currentUserAccessor.UserId;
        if (userId is null)
            return Unauthorized();

        var sessions = await dbContext.WorkSessions
            .AsNoTracking()
            .Where(session => session.UserId == userId.Value)
            .OrderByDescending(session => session.StartTimeUtc)
            .Select(MapToListItem())
            .ToListAsync();

        return Ok(sessions);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await workSessionService.GetWorkSessionDetailsAsync(id);

        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await workSessionService.DeleteAsync(id);

        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error });
        }

        return NoContent();
    }

    [HttpPut("{sessionId:guid}/segments/{segmentId:guid}")]
    public async Task<IActionResult> UpdateSegment(
        Guid sessionId,
        Guid segmentId,
        [FromBody] UpdateWorkSessionSegmentRequest request)
    {
        var result = await workSessionService.UpdateSegmentAsync(sessionId, segmentId, request);

        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    private static Expression<Func<TimeOn.Domain.Entities.WorkSession, WorkSessionListItemDto>> MapToListItem() =>
        session => new WorkSessionListItemDto(
            session.Id,
            session.UserId,
            session.Status.ToString(),
            session.StartTimeUtc,
            session.EndTimeUtc,
            session.TotalDistanceKm);
}

