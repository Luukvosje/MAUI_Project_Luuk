using Microsoft.AspNetCore.Mvc;
using TimeOn.Application.Features.Customers.DTOs;
using TimeOn.Application.Features.Customers.Services;

namespace TimeOn.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _customerService.GetCustomersAsync();
        if (!result.IsSuccess || result.Value is null)
        {
            return BadRequest(new { error = result.Error ?? "Could not load customers." });
        }

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequestDto request)
    {
        var result = await _customerService.CreateCustomerAsync(request);
        if (!result.IsSuccess || result.Value is null)
        {
            return BadRequest(new { error = result.Error ?? "Could not create customer." });
        }

        return CreatedAtAction(nameof(GetAll), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerRequestDto request)
    {
        var result = await _customerService.UpdateCustomerAsync(id, request);
        if (!result.IsSuccess || result.Value is null)
        {
            return BadRequest(new { error = result.Error ?? "Could not update customer." });
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _customerService.DeleteCustomerAsync(id);
        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error ?? "Customer not found." });
        }

        return NoContent();
    }
}
