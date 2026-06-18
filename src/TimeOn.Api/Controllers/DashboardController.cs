using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeOn.Application.Features.Dashboard.Services;

namespace TimeOn.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] int utcOffsetMinutes)
    {
        if (utcOffsetMinutes is < -840 or > 840)
        {
            return BadRequest(new { error = "utcOffsetMinutes must be between -840 and 840." });
        }

        var result = await dashboardService.GetSummaryAsync(utcOffsetMinutes);
        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }
}
