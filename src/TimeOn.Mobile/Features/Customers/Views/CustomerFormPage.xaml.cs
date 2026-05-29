using TimeOn.Application.Features.Customers.DTOs;
using TimeOn.Mobile.Features.Customers.ViewModels;

namespace TimeOn.Mobile.Features.Customers.Views;

public partial class CustomerFormPage : ContentPage, IQueryAttributable
{
    private readonly CustomerFormViewModel _viewModel;

    public CustomerFormPage(CustomerFormViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        _viewModel.InitializeForCreate();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("customer", out var customerObject) && customerObject is CustomerDto customer)
        {
            _viewModel.InitializeForEdit(customer);
            return;
        }

        _viewModel.InitializeForCreate();
    }
}
