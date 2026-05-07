using Microsoft.AspNetCore.Mvc;
using TimeOn.Mobile.Api.Contracts;

namespace TimeOn.Mobile.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Email and password are required.");
        }

        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        var response = new LoginResponse(token, DateTimeOffset.UtcNow.AddHours(8));
        return Ok(response);
    }
}
