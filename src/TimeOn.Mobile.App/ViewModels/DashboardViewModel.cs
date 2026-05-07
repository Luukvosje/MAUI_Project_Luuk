using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeOn.Mobile.Core.Interfaces;

namespace TimeOn.Mobile.App.ViewModels;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly ITrackingService trackingService;

    [ObservableProperty]
    private string trackingState = "Tracking inactive";

    public DashboardViewModel(ITrackingService trackingService)
    {
        this.trackingService = trackingService;
        Title = "Dashboard";
    }

    [RelayCommand]
    private async Task StartDayAsync()
    {
        await trackingService.StartWorkDayAsync();
        TrackingState = trackingService.IsTracking ? "Tracking active" : "Tracking inactive";
    }

    [RelayCommand]
    private async Task StopDayAsync()
    {
        await trackingService.StopWorkDayAsync();
        TrackingState = trackingService.IsTracking ? "Tracking active" : "Tracking inactive";
    }
}
