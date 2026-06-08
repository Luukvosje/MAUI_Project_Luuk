using System.Collections;
using System.Collections.Specialized;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using TimeOn.Mobile.Features.Customers.Models;
using TimeOn.Mobile.Features.Customers.Services;
using MapsMap = Microsoft.Maui.Controls.Maps.Map;

namespace TimeOn.Mobile.Features.Customers.Views;

public partial class CustomersMapView : ContentView
{
    private const double DefaultLatitude = 52.1326;
    private const double DefaultLongitude = 5.2913;
    private const double DefaultRadiusKm = 180;

    private readonly Dictionary<Pin, Guid> _pinLookup = [];
    private readonly MapsMap? _customersMap;

    public static readonly BindableProperty IsNativeMapVisibleProperty =
        BindableProperty.Create(nameof(IsNativeMapVisible), typeof(bool), typeof(CustomersMapView), false);

    public static readonly BindableProperty IsWebMapVisibleProperty =
        BindableProperty.Create(nameof(IsWebMapVisible), typeof(bool), typeof(CustomersMapView), false);

    public static readonly BindableProperty WebMapHtmlProperty =
        BindableProperty.Create(
            nameof(WebMapHtml),
            typeof(string),
            typeof(CustomersMapView),
            propertyChanged: OnWebMapHtmlChanged);

    public static readonly BindableProperty MarkersProperty =
        BindableProperty.Create(
            nameof(Markers),
            typeof(IEnumerable),
            typeof(CustomersMapView),
            propertyChanged: OnMarkersChanged);

    public static readonly BindableProperty MarkerSelectedCommandProperty =
        BindableProperty.Create(nameof(MarkerSelectedCommand), typeof(ICommand), typeof(CustomersMapView));

    public bool IsNativeMapVisible
    {
        get => (bool)GetValue(IsNativeMapVisibleProperty);
        set => SetValue(IsNativeMapVisibleProperty, value);
    }

    public bool IsWebMapVisible
    {
        get => (bool)GetValue(IsWebMapVisibleProperty);
        set => SetValue(IsWebMapVisibleProperty, value);
    }

    public string? WebMapHtml
    {
        get => (string?)GetValue(WebMapHtmlProperty);
        set => SetValue(WebMapHtmlProperty, value);
    }

    public IEnumerable? Markers
    {
        get => (IEnumerable?)GetValue(MarkersProperty);
        set => SetValue(MarkersProperty, value);
    }

    public ICommand? MarkerSelectedCommand
    {
        get => (ICommand?)GetValue(MarkerSelectedCommandProperty);
        set => SetValue(MarkerSelectedCommandProperty, value);
    }

    public CustomersMapView()
    {
        InitializeComponent();

        if (DeviceInfo.Platform != DevicePlatform.WinUI &&
            DeviceInfo.Platform != DevicePlatform.Android)
        {
            _customersMap = new MapsMap();
            NativeMapHost.Children.Add(_customersMap);
            _customersMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Location(DefaultLatitude, DefaultLongitude),
                Distance.FromKilometers(DefaultRadiusKm)));
        }

        CustomersWebMap.Navigating += OnCustomersWebMapNavigating;
    }

    private static void OnWebMapHtmlChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not CustomersMapView view)
        {
            return;
        }

        view.CustomersWebMap.Source = newValue is string html
            ? new HtmlWebViewSource { Html = html }
            : null;
    }

    private static void OnMarkersChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not CustomersMapView view)
        {
            return;
        }

        if (oldValue is INotifyCollectionChanged oldCollection)
        {
            oldCollection.CollectionChanged -= view.OnMarkersCollectionChanged;
        }

        if (newValue is INotifyCollectionChanged newCollection)
        {
            newCollection.CollectionChanged += view.OnMarkersCollectionChanged;
        }

        view.RefreshNativePins();
    }

    private void OnMarkersCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) =>
        RefreshNativePins();

    private void RefreshNativePins()
    {
        if (_customersMap is null || Markers is null)
        {
            return;
        }

        _customersMap.Pins.Clear();
        _pinLookup.Clear();

        foreach (var item in Markers)
        {
            if (item is not CustomerMapMarker marker)
            {
                continue;
            }

            var pin = new Pin
            {
                Label = marker.Name,
                Address = marker.Address,
                Location = new Location(marker.Latitude, marker.Longitude),
                Type = PinType.Place
            };

            pin.MarkerClicked += OnPinMarkerClicked;
            _customersMap.Pins.Add(pin);
            _pinLookup[pin] = marker.Id;
        }
    }

    private async void OnPinMarkerClicked(object? sender, PinClickedEventArgs e)
    {
        if (sender is not Pin pin || !_pinLookup.TryGetValue(pin, out var customerId))
        {
            return;
        }

        e.HideInfoWindow = true;
        await ExecuteMarkerSelectedAsync(customerId);
    }

    private async void OnCustomersWebMapNavigating(object? sender, WebNavigatingEventArgs e)
    {
        var presentation = Handler?.MauiContext?.Services.GetService<ICustomersMapPresentationService>();
        if (presentation is null || !presentation.TryParseMarkerNavigationUrl(e.Url, out var customerId))
        {
            return;
        }

        e.Cancel = true;
        await ExecuteMarkerSelectedAsync(customerId);
    }

    private async Task ExecuteMarkerSelectedAsync(Guid customerId)
    {
        if (MarkerSelectedCommand is IAsyncRelayCommand<Guid> asyncCommand)
        {
            await asyncCommand.ExecuteAsync(customerId);
            return;
        }

        if (MarkerSelectedCommand?.CanExecute(customerId) == true)
        {
            MarkerSelectedCommand.Execute(customerId);
        }
    }
}
