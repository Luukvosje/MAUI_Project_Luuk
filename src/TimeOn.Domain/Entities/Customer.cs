using System.Net.Mail;
using TimeOn.Domain.Constants;
using TimeOn.Domain.Exceptions;
using TimeOn.Domain.Shared;
using TimeOn.Domain.ValueObjects;

namespace TimeOn.Domain.Entities;


public sealed class Customer : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string? Address { get; private set; }
    public string? ContactEmail { get; private set; }
    public bool IsActive { get; private set; }
    public Coordinate Location { get; private set; } = null!;

    public DateTime LastSyncedAtUtc { get; private set; }

    private Customer()
    {
    }

    private Customer(
        Guid id,
        string name,
        string? contactEmail,
        string? address,
        bool isActive,
        Coordinate location,
        DateTime lastSyncedAtUtc) : base(id)
    {
        Name = name;
        ContactEmail = contactEmail;
        Address = address;
        IsActive = isActive;
        Location = location;
        LastSyncedAtUtc = lastSyncedAtUtc;
    }

    public static Customer Create(
        Guid id,
        string name,
        string? contactEmail,
        string? address,
        bool isActive,
        Coordinate location)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Customer id is required.");
        }

        ValidateName(name);
        ValidateContactEmail(contactEmail);
        ValidateAddress(address, location);

        return new Customer(
            id,
            name.Trim(),
            contactEmail?.Trim(),
            address?.Trim(),
            isActive,
            location,
            DateTime.UtcNow);
    }
    public void UpdateDetails(
        string name,
        string? contactEmail,
        string? address,
        bool isActive,
        Coordinate location)
    {
        ValidateName(name);
        ValidateContactEmail(contactEmail);
        ValidateAddress(address, location);

        Name = name.Trim();
        ContactEmail = contactEmail?.Trim();
        Address = address?.Trim();
        IsActive = isActive;
        Location = location;
    }
    public Distance DistanceTo(Coordinate coordinate) => Distance.Between(Location, coordinate);
    public bool IsWithinProximity(Coordinate coordinate)
    {
        ArgumentNullException.ThrowIfNull(coordinate);
        return DistanceTo(coordinate).Meters <= TrackingConstants.CustomerProximityRadiusMeters;
    }
    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Customer name is required.");
        }

        if (name.Trim().Length > TrackingConstants.MaxCustomerNameLength)
        {
            throw new DomainException(
                $"Customer name cannot exceed {TrackingConstants.MaxCustomerNameLength} characters.");
        }
    }
    private static void ValidateAddress(string? address, Coordinate location)
    {
        if (address is not null && address.Trim().Length > TrackingConstants.MaxCustomerAddressLength)
        {
            throw new DomainException(
                $"Customer address cannot exceed {TrackingConstants.MaxCustomerAddressLength} characters.");
        }

        ArgumentNullException.ThrowIfNull(location);
    }
    private static void ValidateContactEmail(string? contactEmail)
    {
        if (string.IsNullOrWhiteSpace(contactEmail))
        {
            return;
        }

        var trimmed = contactEmail.Trim();
        if (trimmed.Length > TrackingConstants.MaxCustomerContactEmailLength)
        {
            throw new DomainException(
                $"Customer contact email cannot exceed {TrackingConstants.MaxCustomerContactEmailLength} characters.");
        }

        try
        {
            _ = new MailAddress(trimmed);
        }
        catch (FormatException)
        {
            throw new DomainException("Customer contact email is invalid.");
        }
    }
}
