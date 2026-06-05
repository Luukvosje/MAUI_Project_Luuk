using FluentAssertions;
using NSubstitute;
using TimeOn.Domain.Shared;
using TimeOn.Application.Features.Customers.DTOs;
using TimeOn.Application.Features.Customers.Services;
using TimeOn.Application.Features.Customers.Validators;
using TimeOn.Application.Interfaces;
using TimeOn.Domain.Entities;
using TimeOn.Domain.ValueObjects;
using TimeOn.Application.Interfaces.Authentication;
using TimeOn.Domain.Interfaces;

namespace TimeOn.UnitTests.Application.Customers;

public class CustomerServiceTests
{
    private readonly ICustomerRepository _customerRepository = Substitute.For<ICustomerRepository>();
    private readonly IGeoLocationService _geoLocationService = Substitute.For<IGeoLocationService>();
    private readonly ICurrentUserAccessor _currentUserAccessor = Substitute.For<ICurrentUserAccessor>();

    private CustomerService CreateSut() =>
        new(
            _customerRepository,
            _geoLocationService,
            new CreateCustomerRequestValidator(),
            new UpdateCustomerRequestValidator(),
            _currentUserAccessor);

    [Fact]
    public async Task GetCustomersAsync_ReturnsMappedCustomers()
    {
        var customer = Customer.Create(
            Guid.NewGuid(),
            "ACME",
            userId: null,
            "contact@acme.com",
            "Street 1",
            true,
            Coordinate.Create(1, 1));
        _customerRepository.GetAllAsync().Returns([customer]);

        var result = await CreateSut().GetCustomersAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value![0].Name.Should().Be("ACME");
        result.Value[0].ContactEmail.Should().Be("contact@acme.com");
        result.Value[0].IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateCustomerAsync_WithValidData_PersistsAndReturnsCustomer()
    {
        var request = new CreateCustomerRequestDto("Customer 1", "customer1@test.com", "Address 1", true);
        _geoLocationService.AdressToCoordinate("Address 1")
            .Returns(Result<Coordinate>.Success(Coordinate.Create(1, 1)));

        var result = await CreateSut().CreateCustomerAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be(request.Name);
        await _customerRepository.Received(1).AddAsync(Arg.Any<Customer>());
    }

    [Fact]
    public async Task CreateCustomerAsync_WithInvalidData_ReturnsFailure()
    {
        var request = new CreateCustomerRequestDto(string.Empty, "not-an-email", "Address", true);
        _geoLocationService.AdressToCoordinate("Address")
            .Returns(Result<Coordinate>.Success(Coordinate.Create(1, 1)));

        var result = await CreateSut().CreateCustomerAsync(request);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateCustomerAsync_WhenMissingCustomer_ReturnsFailure()
    {
        var customerId = Guid.NewGuid();
        _customerRepository.GetByIdAsync(customerId).Returns((Customer?)null);
        var request = new UpdateCustomerRequestDto("New Name", "new@test.com", "Address", false);
        _geoLocationService.AdressToCoordinate("Address")
            .Returns(Result<Coordinate>.Success(Coordinate.Create(1, 1)));

        var result = await CreateSut().UpdateCustomerAsync(customerId, request);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Customer not found.");
    }

    [Fact]
    public async Task UpdateCustomerAsync_WithExistingCustomer_UpdatesCustomer()
    {
        var customerId = Guid.NewGuid();
        var customer = Customer.Create(customerId, "Old", userId: null, "old@test.com", "Old address", true, Coordinate.Create(1, 1));
        _customerRepository.GetByIdAsync(customerId).Returns(customer);
        var request = new UpdateCustomerRequestDto("New", "new@test.com", "New address", false);
        _geoLocationService.AdressToCoordinate("New address")
            .Returns(Result<Coordinate>.Success(Coordinate.Create(2, 2)));

        var result = await CreateSut().UpdateCustomerAsync(customerId, request);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("New");
        result.Value.ContactEmail.Should().Be("new@test.com");
        result.Value.IsActive.Should().BeFalse();
        _customerRepository.Received(1).Update(customer);
    }

    [Fact]
    public async Task DeleteCustomerAsync_WhenMissingCustomer_ReturnsFailure()
    {
        var customerId = Guid.NewGuid();
        _customerRepository.GetByIdAsync(customerId).Returns((Customer?)null);

        var result = await CreateSut().DeleteCustomerAsync(customerId);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Customer not found.");
    }

    [Fact]
    public async Task DeleteCustomerAsync_WithExistingCustomer_DeletesCustomer()
    {
        var customer = Customer.Create(Guid.NewGuid(), "Del", userId: null, "del@test.com", "Address", true, Coordinate.Create(1, 1));
        _customerRepository.GetByIdAsync(customer.Id).Returns(customer);

        var result = await CreateSut().DeleteCustomerAsync(customer.Id);

        result.IsSuccess.Should().BeTrue();
        _customerRepository.Received(1).Delete(customer);
    }
}
