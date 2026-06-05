using TimeOn.Application.Features.Customers.DTOs;
using TimeOn.Mobile.Features.Customers.Models;

namespace TimeOn.Mobile.Features.Customers.Services;

public interface ICustomersMapPresentationService
{
    IReadOnlyList<CustomerMapMarker> BuildMarkers(IEnumerable<CustomerDto> customers);

    string BuildLeafletHtml(IReadOnlyList<CustomerMapMarker> markers);

    bool TryParseMarkerNavigationUrl(string? url, out Guid customerId);
}
