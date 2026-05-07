using Microsoft.AspNetCore.Mvc;
using TimeOn.Mobile.Api.Contracts;
using TimeOn.Mobile.Api.Data;

namespace TimeOn.Mobile.Api.Controllers;

[ApiController]
[Route("api/trips")]
public sealed class TripsController(InMemoryDataStore store) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<TripDto>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyCollection<TripDto>> GetTrips()
    {
        return Ok(store.Trips);
    }

    [HttpPost]
    [ProducesResponseType<TripDto>(StatusCodes.Status201Created)]
    public ActionResult<TripDto> CreateTrip([FromBody] TripDto request)
    {
        var trip = request with { Id = request.Id == Guid.Empty ? Guid.NewGuid() : request.Id };
        store.AddTrip(trip);
        return CreatedAtAction(nameof(GetTrips), new { id = trip.Id }, trip);
    }
}
