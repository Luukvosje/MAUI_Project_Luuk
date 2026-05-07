using Microsoft.AspNetCore.Mvc;
using TimeOn.Mobile.Api.Contracts;
using TimeOn.Mobile.Api.Data;

namespace TimeOn.Mobile.Api.Controllers;

[ApiController]
[Route("api/overview")]
public sealed class OverviewController(InMemoryDataStore store) : ControllerBase
{
    [HttpGet("{day}")]
    [ProducesResponseType<DayOverviewResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<DayOverviewResponse> GetDayOverview(string day)
    {
        if (!DateOnly.TryParse(day, out var parsedDay))
        {
            return BadRequest("Day must have format yyyy-MM-dd.");
        }

        var totalDistance = store.Trips
            .Where(trip => DateOnly.FromDateTime(trip.StartTime.UtcDateTime) == parsedDay)
            .Sum(trip => trip.DistanceKm);

        var visitsCount = store.Visits
            .Count(visit => DateOnly.FromDateTime(visit.ArrivedAt.UtcDateTime) == parsedDay);

        return Ok(new DayOverviewResponse(parsedDay, totalDistance, visitsCount));
    }
}
