using TimeOn.Maui.Features.Tracking.ViewModels;

namespace TimeOn.Maui.Features.Tracking.Views;

public partial class TrackingPage : ContentPage
{
    public TrackingPage(TrackingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is TrackingViewModel viewModel)
        {
            viewModel.OnAppearing();
        }
    }
}
