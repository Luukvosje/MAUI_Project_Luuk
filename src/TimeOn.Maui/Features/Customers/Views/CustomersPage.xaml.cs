using System.ComponentModel;
using TimeOn.Maui.Features.Customers.ViewModels;

namespace TimeOn.Maui.Features.Customers.Views;

public partial class CustomersPage : ContentPage
{
    private readonly CustomersViewModel _viewModel;

    public CustomersPage(CustomersViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        _viewModel.PropertyChanged += OnViewModelPropertyChanged;

        if (_viewModel.ShouldReloadOnAppear() && !_viewModel.LoadCustomersCommand.IsRunning)
        {
            await _viewModel.LoadCustomersCommand.ExecuteAsync(null);
        }
    }

    protected override void OnDisappearing()
    {
        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        base.OnDisappearing();
    }

    private async void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(CustomersViewModel.ErrorMessage))
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(_viewModel.ErrorMessage))
        {
            await DisplayAlertAsync("Error", _viewModel.ErrorMessage, "OK");
        }
    }
}
