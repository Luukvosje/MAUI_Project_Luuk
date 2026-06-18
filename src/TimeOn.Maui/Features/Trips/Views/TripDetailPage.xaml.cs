using System.ComponentModel;
using TimeOn.Maui.Features.Trips.ViewModels;

namespace TimeOn.Maui.Features.Trips.Views;

public partial class TripDetailPage : ContentPage, IQueryAttributable
{
    private readonly TripDetailViewModel _viewModel;

    public TripDetailPage(TripDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("sessionId", out var sessionIdObject) && sessionIdObject is Guid sessionId)
        {
            _viewModel.SessionId = sessionId;
        }
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
        if (e.PropertyName != nameof(TripDetailViewModel.ErrorMessage))
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(_viewModel.ErrorMessage))
        {
            await DisplayAlertAsync("Fout", _viewModel.ErrorMessage, "OK");
        }
    }
}
