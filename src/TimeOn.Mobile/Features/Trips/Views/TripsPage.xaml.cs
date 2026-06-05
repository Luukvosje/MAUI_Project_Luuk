using System.ComponentModel;
using TimeOn.Mobile.Features.Trips.ViewModels;

namespace TimeOn.Mobile.Features.Trips.Views;

public partial class TripsPage : ContentPage
{
    private readonly TripsViewModel _viewModel;

    public TripsPage(TripsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        await _viewModel.LoadOnAppearAsync();
    }

    protected override void OnDisappearing()
    {
        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        base.OnDisappearing();
    }

    private async void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(TripsViewModel.ErrorMessage))
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(_viewModel.ErrorMessage))
        {
            await DisplayAlertAsync("Error", _viewModel.ErrorMessage, "OK");
        }
    }
}
