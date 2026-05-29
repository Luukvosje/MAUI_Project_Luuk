using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using System.Collections.Specialized;
using System.Globalization;
using TimeOn.Mobile.Features.Customers.ViewModels;
using System.ComponentModel;
using System.Text;
using MapsMap = Microsoft.Maui.Controls.Maps.Map;

namespace TimeOn.Mobile.Features.Customers.Views;

public partial class CustomersPage : ContentPage
{
    private readonly CustomersViewModel _viewModel;
    private readonly Dictionary<Pin, Guid> _pinLookup = [];
    private readonly MapsMap? _customersMap;

    public CustomersPage(CustomersViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        _viewModel.Customers.CollectionChanged += OnCustomersCollectionChanged;
        CustomersWebMap.Navigating += OnCustomersWebMapNavigating;

        if (!_viewModel.IsWindowsPlatform)
        {
            _customersMap = new MapsMap();
            NativeMapHost.Children.Add(_customersMap);

            _customersMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Location(52.1326, 5.2913),
                Distance.FromKilometers(180)));
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_viewModel.Customers.Count == 0 && !_viewModel.LoadCustomersCommand.IsRunning)
        {
            await _viewModel.LoadCustomersCommand.ExecuteAsync(null);
        }

        RefreshMapPins();
    }

    protected override void OnDisappearing()
    {
        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        _viewModel.Customers.CollectionChanged -= OnCustomersCollectionChanged;
        CustomersWebMap.Navigating -= OnCustomersWebMapNavigating;
        base.OnDisappearing();
    }

    private async void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(CustomersViewModel.ErrorMessage))
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(_viewModel.ErrorMessage))
        {
            await DisplayAlertAsync("Error", _viewModel.ErrorMessage, "OK");
        }

        if (e.PropertyName == nameof(CustomersViewModel.IsMapView) && _viewModel.IsMapView)
        {
            RefreshMapPins();
        }
    }

    private void OnCustomersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RefreshMapPins();
    }

    private void RefreshMapPins()
    {
        if (_viewModel.IsWindowsPlatform)
        {
            RefreshWindowsMapHtml();
            return;
        }

        if (_customersMap is null)
        {
            return;
        }

        _customersMap.Pins.Clear();
        _pinLookup.Clear();

        foreach (var customer in _viewModel.Customers)
        {
            if (customer.Latitude == 0 && customer.Longitude == 0)
            {
                continue;
            }

            var pin = new Pin
            {
                Label = customer.Name,
                Address = customer.Address ?? string.Empty,
                Location = new Location(customer.Latitude, customer.Longitude),
                Type = PinType.Place
            };

            pin.MarkerClicked += OnPinMarkerClicked;
            _customersMap.Pins.Add(pin);
            _pinLookup[pin] = customer.Id;
        }
    }

    private void RefreshWindowsMapHtml()
    {
        var markers = new StringBuilder();
        foreach (var customer in _viewModel.Customers)
        {
            if (customer.Latitude == 0 && customer.Longitude == 0)
            {
                continue;
            }

            var lat = customer.Latitude.ToString(CultureInfo.InvariantCulture);
            var lng = customer.Longitude.ToString(CultureInfo.InvariantCulture);
            var name = JsEncode(customer.Name);
            var address = JsEncode(customer.Address ?? string.Empty);
            var id = customer.Id.ToString();

            markers.AppendLine(
                $"L.marker([{lat}, {lng}]).addTo(map).bindPopup('{name}<br/>{address}<br/><a href=\"timeon://customer/{id}\">Open actions</a>');");
        }

        var html = $$"""
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
            const map = L.map('map').setView([52.1326, 5.2913], 7);
            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
              maxZoom: 19,
              attribution: '&copy; OpenStreetMap contributors'
            }).addTo(map);
            {{markers}}
          </script>
        </body>
        </html>
        """;

        CustomersWebMap.Source = new HtmlWebViewSource { Html = html };
    }

    private async void OnCustomersWebMapNavigating(object? sender, WebNavigatingEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.Url) || !e.Url.StartsWith("timeon://customer/", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        e.Cancel = true;
        var idText = e.Url["timeon://customer/".Length..];
        if (!Guid.TryParse(idText, out var customerId))
        {
            return;
        }

        await _viewModel.SelectMapMarkerCommand.ExecuteAsync(customerId);
    }

    private static string JsEncode(string value) =>
        value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("'", "\\'", StringComparison.Ordinal)
            .Replace("\r", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal);

    private async void OnPinMarkerClicked(object? sender, PinClickedEventArgs e)
    {
        if (sender is not Pin pin || !_pinLookup.TryGetValue(pin, out var customerId))
        {
            return;
        }

        e.HideInfoWindow = true;
        await _viewModel.SelectMapMarkerCommand.ExecuteAsync(customerId);
    }
}
