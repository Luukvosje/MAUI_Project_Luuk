using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeOn.Infrastructure.Persistence;

namespace TimeOn.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UsersController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public UsersController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _dbContext.Users
            .AsNoTracking()
            .Select(user => new UserResponse(user.Id, user.Name, user.Email.Value))
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => new UserResponse(item.Id, item.Name, item.Email.Value))
            .FirstOrDefaultAsync();

        return user is null ? NotFound() : Ok(user);
    }

    private sealed record UserResponse(Guid Id, string Name, string Email);
}
