using Microsoft.AspNetCore.Mvc;
using TimeOn.Mobile.Api.Contracts;
using TimeOn.Mobile.Api.Data;

namespace TimeOn.Mobile.Api.Controllers;

[ApiController]
[Route("api/visits")]
public sealed class VisitsController(InMemoryDataStore store) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<VisitDto>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyCollection<VisitDto>> GetVisits()
    {
        return Ok(store.Visits);
    }

    [HttpPost]
    [ProducesResponseType<VisitDto>(StatusCodes.Status201Created)]
    public ActionResult<VisitDto> CreateVisit([FromBody] VisitDto request)
    {
        var visit = request with { Id = request.Id == Guid.Empty ? Guid.NewGuid() : request.Id };
        store.AddVisit(visit);
        return CreatedAtAction(nameof(GetVisits), new { id = visit.Id }, visit);
    }
}
