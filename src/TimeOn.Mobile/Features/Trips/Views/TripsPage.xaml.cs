using TimeOn.Mobile.Features.Trips.ViewModels;

namespace TimeOn.Mobile.Features.Trips.Views;

public partial class TripsPage : ContentPage
{
    public TripsPage(TripsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
