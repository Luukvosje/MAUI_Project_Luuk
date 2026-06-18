namespace TimeOn.Maui.Features.Customers.Models;

public sealed record CustomerMapMarker(
    Guid Id,
    string Name,
    string Address,
    double Latitude,
    double Longitude);
