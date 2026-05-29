using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeOn.Mobile.Interfaces;

namespace TimeOn.Mobile.Features.Tracking.ViewModels;

public partial class TrackingViewModel : ObservableObject
{
    private readonly ILocationTrackingService _locationTrackingService;

    [ObservableProperty]
    public partial bool IsTracking { get; set; }

    public TrackingViewModel(ILocationTrackingService locationTrackingService)
    {
        _locationTrackingService = locationTrackingService;
        IsTracking = locationTrackingService.IsTracking;
    }

    [RelayCommand]
    private async Task ToggleTrackingAsync()
    {
        if (IsTracking)
        {
            await _locationTrackingService.StopTrackingAsync();
        }
        else
        {
            await _locationTrackingService.StartTrackingAsync();
        }

        IsTracking = _locationTrackingService.IsTracking;
    }
}
