using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TimeOn.Application.Features.Customers.DTOs;
using TimeOn.Application.Features.Customers.Services;
using TimeOn.Mobile.Features.Customers.Views;

namespace TimeOn.Mobile.Features.Customers.ViewModels;

public partial class CustomersViewModel : ObservableObject
{
    private readonly ICustomerService _customerService;

    public ObservableCollection<CustomerDto> Customers { get; } = [];

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial CustomerDto? SelectedCustomer { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string SelectedView { get; set; } = "List";

    public bool IsListView => SelectedView == "List";
    public bool IsMapView => SelectedView == "Map";
    public bool IsWindowsPlatform => DeviceInfo.Platform == DevicePlatform.WinUI;
    public bool IsNativeMapVisible => IsMapView && !IsWindowsPlatform;
    public bool IsWebMapVisible => IsMapView && IsWindowsPlatform;

    public CustomersViewModel(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    partial void OnSelectedViewChanged(string value)
    {
        _ = value;
        OnPropertyChanged(nameof(IsListView));
        OnPropertyChanged(nameof(IsMapView));
        OnPropertyChanged(nameof(IsNativeMapVisible));
        OnPropertyChanged(nameof(IsWebMapVisible));
    }

    [RelayCommand]
    private async Task LoadCustomersAsync()
    {
        await ExecuteWithLoading(async () =>
        {
            var result = await _customerService.GetCustomersAsync();
            if (!result.IsSuccess || result.Value is null)
            {
                ErrorMessage = result.Error ?? "Could not load customers.";
                return;
            }

            Customers.Clear();
            foreach (var customer in result.Value)
            {
                Customers.Add(customer);
            }
        });
    }

    [RelayCommand]
    private void ShowList() => SelectedView = "List";

    [RelayCommand]
    private void ShowMap() => SelectedView = "Map";

    [RelayCommand]
    private async Task CreateCustomerAsync()
    {
        await Shell.Current.GoToAsync(nameof(CustomerFormPage));
    }

    [RelayCommand]
    private async Task EditSelectedCustomerAsync()
    {
        if (SelectedCustomer is null)
        {
            ErrorMessage = "Select a customer first.";
            return;
        }

        await NavigateToEditAsync(SelectedCustomer);
    }

    [RelayCommand]
    private async Task DeleteSelectedCustomerAsync()
    {
        if (SelectedCustomer is null)
        {
            ErrorMessage = "Select a customer first.";
            return;
        }

        await DeleteCustomerAsync(SelectedCustomer);
    }

    [RelayCommand]
    private async Task SelectMapMarkerAsync(Guid customerId)
    {
        var customer = Customers.FirstOrDefault(c => c.Id == customerId);
        if (customer is null)
        {
            return;
        }

        SelectedCustomer = customer;

        var action = await Shell.Current.DisplayActionSheetAsync(
            customer.Name,
            "Cancel",
            null,
            "Edit customer",
            "Delete customer");

        if (action == "Edit customer")
        {
            await NavigateToEditAsync(customer);
            return;
        }

        if (action == "Delete customer")
        {
            await DeleteCustomerAsync(customer);
        }
    }

    private async Task ExecuteWithLoading(Func<Task> action)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            await action();
        }
        catch (Exception)
        {
            ErrorMessage = "An unexpected error occurred.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task NavigateToEditAsync(CustomerDto customer)
    {
        await Shell.Current.GoToAsync(nameof(CustomerFormPage), new Dictionary<string, object>
        {
            ["customer"] = customer
        });
    }

    private async Task DeleteCustomerAsync(CustomerDto customer)
    {
        await ExecuteWithLoading(async () =>
        {
            var result = await _customerService.DeleteCustomerAsync(customer.Id);
            if (!result.IsSuccess)
            {
                ErrorMessage = result.Error ?? "Could not delete customer.";
                return;
            }

            Customers.Remove(customer);
            if (SelectedCustomer?.Id == customer.Id)
            {
                SelectedCustomer = null;
            }
        });
    }
}
