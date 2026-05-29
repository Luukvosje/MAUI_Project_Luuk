using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeOn.Application.Features.Customers.DTOs;
using TimeOn.Application.Features.Customers.Services;

namespace TimeOn.Mobile.Features.Customers.ViewModels;

public partial class CustomerFormViewModel : ObservableObject
{
    private readonly ICustomerService _customerService;

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? ContactEmail { get; set; }

    [ObservableProperty]
    public partial string? Address { get; set; }

    [ObservableProperty]
    public partial bool IsActive { get; set; } = true;

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial bool IsEditMode { get; set; }

    [ObservableProperty]
    public partial Guid? CustomerId { get; set; }

    public string Title => IsEditMode ? "Update customer" : "Create customer";

    public CustomerFormViewModel(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public void InitializeForCreate()
    {
        IsEditMode = false;
        CustomerId = null;
        Name = string.Empty;
        ContactEmail = null;
        Address = null;
        IsActive = true;
        ErrorMessage = null;
    }

    public void InitializeForEdit(CustomerDto customer)
    {
        IsEditMode = true;
        CustomerId = customer.Id;
        Name = customer.Name;
        ContactEmail = customer.ContactEmail;
        Address = customer.Address;
        IsActive = customer.IsActive;
        ErrorMessage = null;
    }

    partial void OnIsEditModeChanged(bool value)
    {
        _ = value;
        OnPropertyChanged(nameof(Title));
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await ExecuteWithLoading(async () =>
        {
            if (IsEditMode && CustomerId is { } customerId)
            {
                var update = new UpdateCustomerRequestDto(Name, ContactEmail, Address, IsActive);
                var updateResult = await _customerService.UpdateCustomerAsync(customerId, update);
                if (!updateResult.IsSuccess)
                {
                    ErrorMessage = updateResult.Error ?? "Could not update customer.";
                    return;
                }
            }
            else
            {
                var create = new CreateCustomerRequestDto(Name, ContactEmail, Address, IsActive);
                var createResult = await _customerService.CreateCustomerAsync(create);
                if (!createResult.IsSuccess)
                {
                    ErrorMessage = createResult.Error ?? "Could not create customer.";
                    return;
                }
            }

            await Shell.Current.GoToAsync("..");
        });
    }

    [RelayCommand]
    private static async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
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
}
