namespace TimeOn.Application.Features.Customers.DTOs;

public sealed record CustomerDto(
    Guid Id,
    string Name,
    string? ContactEmail,
    string? Address,
    bool IsActive,
    double Latitude,
    double Longitude);

public sealed record CreateCustomerRequestDto(string Name, string? ContactEmail, string? Address, bool IsActive);
public sealed record UpdateCustomerRequestDto(string Name, string? ContactEmail, string? Address, bool IsActive);
