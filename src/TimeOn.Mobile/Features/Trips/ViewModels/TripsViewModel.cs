using CommunityToolkit.Mvvm.ComponentModel;
using TimeOn.Mobile.Interfaces;

namespace TimeOn.Mobile.Features.Trips.ViewModels;

public partial class TripsViewModel : ObservableObject
{
    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    public TripsViewModel(IApiService apiService)
    {
        _ = apiService;
    }
}
