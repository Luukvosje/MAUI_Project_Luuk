using TimeOn.Mobile.App.ViewModels;

namespace TimeOn.Mobile.App.Views;

public partial class TripsOverviewPage : ContentPage
{
    public TripsOverviewPage(TripsOverviewViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
