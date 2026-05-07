using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using TimeOn.Mobile.Core.Interfaces;
using TimeOn.Mobile.Core.Models;

namespace TimeOn.Mobile.App.ViewModels;

public partial class TripsOverviewViewModel : BaseViewModel
{
    private readonly ITripRepository tripRepository;

    public ObservableCollection<Trip> Trips { get; } = [];

    public TripsOverviewViewModel(ITripRepository tripRepository)
    {
        this.tripRepository = tripRepository;
        Title = "Day Trips";
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var trips = await tripRepository.GetTripsForDayAsync(DateOnly.FromDateTime(DateTime.Now));
            Trips.Clear();
            foreach (var trip in trips)
            {
                Trips.Add(trip);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
