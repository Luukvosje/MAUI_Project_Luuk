using System.Globalization;
using System.Text;
using TimeOn.Application.Features.Customers.DTOs;
using TimeOn.Maui.Features.Customers.Models;

namespace TimeOn.Maui.Features.Customers.Services;

public sealed class CustomersMapPresentationService : ICustomersMapPresentationService
{
    private const double DefaultLatitude = 52.1326;
    private const double DefaultLongitude = 5.2913;
    private const int WindowsZoomLevel = 7;
    private const string MarkerUrlPrefix = "timeon://customer/";

    public IReadOnlyList<CustomerMapMarker> BuildMarkers(IEnumerable<CustomerDto> customers) =>
        customers
            .Where(c => c.Latitude != 0 || c.Longitude != 0)
            .Select(c => new CustomerMapMarker(
                c.Id,
                c.Name,
                c.Address ?? string.Empty,
                c.Latitude,
                c.Longitude))
            .ToList();

    public string BuildLeafletHtml(IReadOnlyList<CustomerMapMarker> markers)
    {
        var markerScript = new StringBuilder();
        foreach (var marker in markers)
        {
            var lat = marker.Latitude.ToString(CultureInfo.InvariantCulture);
            var lng = marker.Longitude.ToString(CultureInfo.InvariantCulture);
            var name = JsEncode(marker.Name);
            var address = JsEncode(marker.Address);
            var id = marker.Id.ToString();

            markerScript.AppendLine(
                $"L.marker([{lat}, {lng}]).addTo(map).bindPopup('{name}<br/>{address}<br/><a href=\"{MarkerUrlPrefix}{id}\">Acties openen</a>');");
        }

        var defaultLat = DefaultLatitude.ToString(CultureInfo.InvariantCulture);
        var defaultLng = DefaultLongitude.ToString(CultureInfo.InvariantCulture);

        return $$"""
        <!doctype html>
        <html>
        <head>
          <meta charset="utf-8" />
          <meta name="viewport" content="width=device-width, initial-scale=1.0" />
          <link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css"/>
          <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>
          <style>html, body, #map { height: 100%; margin: 0; }</style>
        </head>
        <body>
          <div id="map"></div>
          <script>
            const map = L.map('map').setView([{{defaultLat}}, {{defaultLng}}], {{WindowsZoomLevel}});
            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
              maxZoom: 19,
              attribution: '&copy; OpenStreetMap contributors'
            }).addTo(map);
            {{markerScript}}
          </script>
        </body>
        </html>
        """;
    }

    public bool TryParseMarkerNavigationUrl(string? url, out Guid customerId)
    {
        customerId = default;
        if (string.IsNullOrWhiteSpace(url) ||
            !url.StartsWith(MarkerUrlPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var idText = url[MarkerUrlPrefix.Length..];
        return Guid.TryParse(idText, out customerId);
    }

    private static string JsEncode(string value) =>
        value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("'", "\\'", StringComparison.Ordinal)
            .Replace("\r", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal);
}
