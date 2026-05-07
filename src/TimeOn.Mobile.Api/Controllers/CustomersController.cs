using Microsoft.AspNetCore.Mvc;
using TimeOn.Mobile.Api.Contracts;
using TimeOn.Mobile.Api.Data;

namespace TimeOn.Mobile.Api.Controllers;

[ApiController]
[Route("api/customers")]
public sealed class CustomersController(InMemoryDataStore store) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<CustomerDto>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyCollection<CustomerDto>> GetCustomers()
    {
        return Ok(store.Customers);
    }
}
