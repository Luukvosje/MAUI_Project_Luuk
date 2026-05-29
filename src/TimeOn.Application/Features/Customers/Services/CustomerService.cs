using FluentValidation;
using TimeOn.Application.Behaviors;
using TimeOn.Domain.Shared;
using TimeOn.Application.Features.Customers.DTOs;
using TimeOn.Domain.Entities;
using TimeOn.Domain.Exceptions;
using TimeOn.Domain.RepositoryInterfaces;
using TimeOn.Application.Interfaces;
using TimeOn.Domain.ValueObjects;

namespace TimeOn.Application.Features.Customers.Services;

public sealed class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IGeoLocationService _geoLocationService;
    private readonly IValidator<CreateCustomerRequestDto> _createValidator;
    private readonly IValidator<UpdateCustomerRequestDto> _updateValidator;

    public CustomerService(
        ICustomerRepository customerRepository,
        IGeoLocationService geoLocationService,
        IValidator<CreateCustomerRequestDto> createValidator,
        IValidator<UpdateCustomerRequestDto> updateValidator)
    {
        _customerRepository = customerRepository;
        _geoLocationService = geoLocationService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
}

    public async Task<Result<IReadOnlyList<CustomerDto>>> GetCustomersAsync()
    {
        var customers = await _customerRepository.GetAllAsync();
        var mappedCustomers = customers
            .Select(MapToDto)
            .ToList();

        return Result<IReadOnlyList<CustomerDto>>.Success(mappedCustomers);
    }

    public async Task<Result<CustomerDto>> CreateCustomerAsync(CreateCustomerRequestDto request)
    {
        try
        {
            await ValidationBehavior.ValidateAsync(request, _createValidator);
        }
        catch (ValidationException ex)
        {
            return Result<CustomerDto>.Failure(ex.Message);
        }

        var coordinateResult = await ResolveCoordinateAsync(request.Address);
        if (!coordinateResult.IsSuccess || coordinateResult.Value is null)
        {
            return Result<CustomerDto>.Failure(coordinateResult.Error ?? "Unable to resolve address coordinates.");
        }

        Customer customer;
        try
        {
            customer = Customer.Create(
                Guid.NewGuid(),
                request.Name,
                request.ContactEmail,
                request.Address,
                request.IsActive,
                coordinateResult.Value);
        }
        catch (DomainException ex)
        {
            return Result<CustomerDto>.Failure(ex.Message);
        }

        await _customerRepository.AddAsync(customer);
        return Result<CustomerDto>.Success(MapToDto(customer));
    }

    public async Task<Result<CustomerDto>> UpdateCustomerAsync(Guid customerId, UpdateCustomerRequestDto request)
    {
        try
        {
            await ValidationBehavior.ValidateAsync(request, _updateValidator);
        }
        catch (ValidationException ex)
        {
            return Result<CustomerDto>.Failure(ex.Message);
        }

        var coordinateResult = await ResolveCoordinateAsync(request.Address);
        if (!coordinateResult.IsSuccess || coordinateResult.Value is null)
        {
            return Result<CustomerDto>.Failure(coordinateResult.Error ?? "Unable to resolve address coordinates.");
        }

        var customer = await _customerRepository.GetByIdAsync(customerId);
        if (customer is null)
        {
            return Result<CustomerDto>.Failure("Customer not found.");
        }

        try
        {
            customer.UpdateDetails(
                request.Name,
                request.ContactEmail,
                request.Address,
                request.IsActive,
                coordinateResult.Value);
        }
        catch (DomainException ex)
        {
            return Result<CustomerDto>.Failure(ex.Message);
        }

        _customerRepository.Update(customer);
        return Result<CustomerDto>.Success(MapToDto(customer));
    }

    public async Task<Result> DeleteCustomerAsync(Guid customerId)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId);
        if (customer is null)
        {
            return Result.Failure("Customer not found.");
        }

        _customerRepository.Delete(customer);
        return Result.Success();
    }

    private static CustomerDto MapToDto(Customer customer) =>
        new(
            customer.Id,
            customer.Name,
            customer.ContactEmail,
            customer.Address,
            customer.IsActive,
            customer.Location.Latitude,
            customer.Location.Longitude);

    private async Task<Result<Coordinate>> ResolveCoordinateAsync(string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return Result<Coordinate>.Success(Coordinate.Create(0, 0));
        }

        return await _geoLocationService.AdressToCoordinate(address.Trim());
    }
    }
