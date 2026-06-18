using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TimeOn.Application.Features.Customers.DTOs;
using TimeOn.Application.Features.Customers.Services;
using TimeOn.Maui.Features.Customers.Views;

namespace TimeOn.Maui.Features.Customers.ViewModels;

public partial class CustomersViewModel : ObservableObject
{
    private readonly ICustomerService _customerService;
    private readonly CustomersMapViewModel _mapViewModel;
    private readonly CustomerFormViewModel _formViewModel;
    private bool _reloadOnNextAppear;

    public ObservableCollection<CustomerDto> Customers { get; } = [];

    public CustomersMapViewModel Map => _mapViewModel;

    public CustomerFormViewModel Form => _formViewModel;

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedCustomer))]
    [NotifyPropertyChangedFor(nameof(SelectedCustomerName))]
    public partial CustomerDto? SelectedCustomer { get; set; }

    public bool HasSelectedCustomer => SelectedCustomer is not null;

    public string? SelectedCustomerName => SelectedCustomer?.Name;

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string SelectedView { get; set; } = "List";

    public bool IsListView => SelectedView == "List";
    public bool IsMapView => SelectedView == "Map";
    public bool IsCreateView => SelectedView == "Create";
    public bool UseWebMap =>
        DeviceInfo.Platform == DevicePlatform.WinUI ||
        DeviceInfo.Platform == DevicePlatform.Android;
    public bool IsNativeMapVisible => IsMapView && !UseWebMap;
    public bool IsWebMapVisible => IsMapView && UseWebMap;

    public CustomersViewModel(
        ICustomerService customerService,
        CustomersMapViewModel mapViewModel,
        CustomerFormViewModel formViewModel)
    {
        _customerService = customerService;
        _mapViewModel = mapViewModel;
        _formViewModel = formViewModel;
        _formViewModel.IsEmbedded = true;
        Customers.CollectionChanged += (_, _) => _mapViewModel.Refresh(Customers);
    }

    public void MarkReloadOnReturn()
    {
        _reloadOnNextAppear = true;
    }

    public bool ShouldReloadOnAppear()
    {
        if (_reloadOnNextAppear)
        {
            _reloadOnNextAppear = false;
            return true;
        }

        return Customers.Count == 0;
    }

    partial void OnSelectedViewChanged(string value)
    {
        _ = value;
        OnPropertyChanged(nameof(IsListView));
        OnPropertyChanged(nameof(IsMapView));
        OnPropertyChanged(nameof(IsCreateView));
        OnPropertyChanged(nameof(IsNativeMapVisible));
        OnPropertyChanged(nameof(IsWebMapVisible));

        if (value == "Create")
        {
            _formViewModel.InitializeForCreate();
        }

        if (IsMapView)
        {
            _mapViewModel.Refresh(Customers);
        }
    }

    [RelayCommand]
    private async Task LoadCustomersAsync()
    {
        await ExecuteWithLoading(async () =>
        {
            var result = await _customerService.GetCustomersAsync();
            if (!result.IsSuccess || result.Value is null)
            {
                ErrorMessage = result.Error ?? "Kon klanten niet laden.";
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
    private void ShowCreate() => SelectedView = "Create";

    [RelayCommand]
    private async Task SaveCreateCustomerAsync()
    {
        await _formViewModel.SaveCommand.ExecuteAsync(null);
        if (!string.IsNullOrWhiteSpace(_formViewModel.ErrorMessage))
        {
            return;
        }

        SelectedView = "List";
        await LoadCustomersAsync();
    }

    [RelayCommand]
    private void CancelCreate() => SelectedView = "List";

    [RelayCommand]
    private async Task CreateCustomerAsync()
    {
        MarkReloadOnReturn();
        await Shell.Current.GoToAsync(nameof(CustomerFormPage));
    }

    [RelayCommand]
    private async Task EditSelectedCustomerAsync()
    {
        if (SelectedCustomer == null)
        {
            ErrorMessage = "Selecteer eerst een klant.";
            return;
        }

        await NavigateToEditAsync(SelectedCustomer);
    }

    [RelayCommand]
    private async Task DeleteSelectedCustomerAsync()
    {
        if (SelectedCustomer == null)
        {
            ErrorMessage = "Selecteer eerst een klant.";
            return;
        }

        await DeleteCustomerAsync(SelectedCustomer);
    }

    [RelayCommand]
    private Task SelectMapMarkerAsync(Guid customerId)
    {
        var customer = Customers.FirstOrDefault(c => c.Id == customerId);
        if (customer is not null)
        {
            SelectedCustomer = customer;
            ErrorMessage = null;
        }

        return Task.CompletedTask;
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
            ErrorMessage = "Er is een onverwachte fout opgetreden.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task NavigateToEditAsync(CustomerDto customer)
    {
        MarkReloadOnReturn();
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
                ErrorMessage = result.Error ?? "Kon klant niet verwijderen.";
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
