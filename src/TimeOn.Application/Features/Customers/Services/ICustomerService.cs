using TimeOn.Domain.Shared;
using TimeOn.Application.Features.Customers.DTOs;

namespace TimeOn.Application.Features.Customers.Services;

public interface ICustomerService
{
    Task<Result<IReadOnlyList<CustomerDto>>> GetCustomersAsync();
    Task<Result<CustomerDto>> CreateCustomerAsync(CreateCustomerRequestDto request);
    Task<Result<CustomerDto>> UpdateCustomerAsync(Guid customerId, UpdateCustomerRequestDto request);
    Task<Result> DeleteCustomerAsync(Guid customerId);
}
