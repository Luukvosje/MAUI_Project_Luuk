using TimeOn.Domain.Shared;
using TimeOn.Application.Features.Customers.DTOs;
using TimeOn.Application.Features.Customers.Services;
using TimeOn.Maui.Interfaces;

namespace TimeOn.Maui.Services;

public sealed class RemoteCustomerService : ICustomerService
{
    private const string CustomersEndpoint = "api/customers";
    private readonly IApiService _apiService;

    public RemoteCustomerService(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<Result<IReadOnlyList<CustomerDto>>> GetCustomersAsync()
    {
        try
        {
            var customers = await _apiService.GetAsync<IReadOnlyList<CustomerDto>>(CustomersEndpoint);
            return customers is null
                ? Result<IReadOnlyList<CustomerDto>>.Failure(_apiService.LastError ?? "Could not load customers.")
                : Result<IReadOnlyList<CustomerDto>>.Success(customers);
        }
        catch (Exception)
        {
            return Result<IReadOnlyList<CustomerDto>>.Failure(_apiService.LastError ?? "Could not load customers.");
        }
    }

    public async Task<Result<CustomerDto>> CreateCustomerAsync(CreateCustomerRequestDto request)
    {
        try
        {
            var response = await _apiService.PostAsync<CreateCustomerRequestDto, CustomerDto>(CustomersEndpoint, request);
            return response is null
                ? Result<CustomerDto>.Failure(_apiService.LastError ?? "Could not create customer.")
                : Result<CustomerDto>.Success(response);
        }
        catch (Exception)
        {
            return Result<CustomerDto>.Failure(_apiService.LastError ?? "Could not create customer.");
        }
    }

    public async Task<Result<CustomerDto>> UpdateCustomerAsync(Guid customerId, UpdateCustomerRequestDto request)
    {
        try
        {
            var response = await _apiService.PutAsync<UpdateCustomerRequestDto, CustomerDto>($"{CustomersEndpoint}/{customerId}", request);
            return response is null
                ? Result<CustomerDto>.Failure(_apiService.LastError ?? "Could not update customer.")
                : Result<CustomerDto>.Success(response);
        }
        catch (Exception)
        {
            return Result<CustomerDto>.Failure(_apiService.LastError ?? "Could not update customer.");
        }
    }

    public async Task<Result> DeleteCustomerAsync(Guid customerId)
    {
        try
        {
            await _apiService.DeleteAsync($"{CustomersEndpoint}/{customerId}");
            return _apiService.LastError is null
                ? Result.Success()
                : Result.Failure(_apiService.LastError);
        }
        catch (Exception)
        {
            return Result.Failure(_apiService.LastError ?? "Could not delete customer.");
        }
    }
}
