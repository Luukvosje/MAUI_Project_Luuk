using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using TimeOn.Application.Features.Customers.DTOs;
using TimeOn.Maui.Features.Customers.Models;
using TimeOn.Maui.Features.Customers.Services;

namespace TimeOn.Maui.Features.Customers.ViewModels;

public partial class CustomersMapViewModel : ObservableObject
{
    private readonly ICustomersMapPresentationService _presentationService;

    public ObservableCollection<CustomerMapMarker> Markers { get; } = [];

    [ObservableProperty]
    public partial string? WebMapHtml { get; private set; }

    public CustomersMapViewModel(ICustomersMapPresentationService presentationService)
    {
        _presentationService = presentationService;
    }

    public void Refresh(IEnumerable<CustomerDto> customers)
    {
        var markers = _presentationService.BuildMarkers(customers);

        Markers.Clear();
        foreach (var marker in markers)
        {
            Markers.Add(marker);
        }

        WebMapHtml = _presentationService.BuildLeafletHtml(markers);
    }
}
