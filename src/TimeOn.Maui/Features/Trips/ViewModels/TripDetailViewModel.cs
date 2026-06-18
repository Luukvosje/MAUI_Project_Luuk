using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TimeOn.Application.Features.Customers.Services;
using TimeOn.Application.Features.WorkSessions.DTOs;
using TimeOn.Application.Features.WorkSessions.Services;

namespace TimeOn.Maui.Features.Trips.ViewModels;

public partial class TripDetailViewModel : ObservableObject
{
    private static readonly CultureInfo DutchCulture = new("nl-NL");

    private readonly IWorkSessionService _workSessionService;
    private readonly ICustomerService _customerService;

    [ObservableProperty]
    public partial Guid SessionId { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string Status { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StartLabel { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string EndLabel { get; set; } = string.Empty;

    [ObservableProperty]
    public partial double TotalDistanceKm { get; set; }

    public ObservableCollection<SegmentListItem> Segments { get; } = [];

    public ObservableCollection<CustomerPickerItem> CustomerOptions { get; } = [];

    public string Title { get; private set; } = "Rit";

    public TripDetailViewModel(
        IWorkSessionService workSessionService,
        ICustomerService customerService)
    {
        _workSessionService = workSessionService;
        _customerService = customerService;
    }

    public async Task LoadOnAppearAsync()
    {
        if (SessionId == Guid.Empty)
        {
            ErrorMessage = "Rit-id ontbreekt.";
            return;
        }

        if (!LoadTripCommand.IsRunning)
        {
            await LoadTripCommand.ExecuteAsync(null);
        }
    }

    [RelayCommand]
    private async Task LoadTripAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            await LoadCustomerOptionsAsync();

            var result = await _workSessionService.GetWorkSessionDetailsAsync(SessionId);
            if (!result.IsSuccess || result.Value is null)
            {
                ErrorMessage = result.Error ?? "Kon rit niet laden.";
                return;
            }

            var trip = result.Value;
            Status = TranslateStatus(trip.Status);
            StartLabel = ToLocalTime(trip.StartTimeUtc).ToString("dd MMM yyyy HH:mm", DutchCulture);
            EndLabel = trip.EndTimeUtc is { } endUtc
                ? ToLocalTime(endUtc).ToString("dd MMM yyyy HH:mm", DutchCulture)
                : "Bezig";
            TotalDistanceKm = trip.TotalDistanceKm;
            Title = $"Rit {ToLocalTime(trip.StartTimeUtc):dd MMM yyyy}";

            Segments.Clear();
            foreach (var segment in trip.Segments)
            {
                Segments.Add(MapSegment(segment));
            }

            foreach (var segment in Segments.Where(s => s.IsStationary))
            {
                segment.SelectedCustomer = ResolveCustomerSelection(segment.CustomerId);
            }

            OnPropertyChanged(nameof(Title));
        });
    }

    [RelayCommand]
    private async Task SaveSegmentAsync(SegmentListItem? segment)
    {
        if (segment is null || SessionId == Guid.Empty)
        {
            return;
        }

        if (segment.IsDriving &&
            !double.TryParse(segment.DistanceKmText, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
        {
            ErrorMessage = "Voer een geldige afstand in kilometers in.";
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            double? distanceKm = null;
            if (segment.IsDriving)
            {
                distanceKm = double.Parse(segment.DistanceKmText, CultureInfo.InvariantCulture);
            }

            var request = new UpdateWorkSessionSegmentRequest(
                ToUtc(segment.StartLocal),
                ToUtc(segment.EndLocal),
                distanceKm,
                segment.IsStationary ? segment.SelectedCustomer?.Id ?? segment.CustomerId : null);

            var result = await _workSessionService.UpdateSegmentAsync(SessionId, segment.Id, request);
            if (!result.IsSuccess || result.Value is null)
            {
                ErrorMessage = result.Error ?? "Kon segment niet opslaan.";
                return;
            }

            ApplySavedSegment(segment, result.Value);
            segment.SelectedCustomer = ResolveCustomerSelection(result.Value.CustomerId);
            await RefreshTripSummaryAsync();
        });
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        var confirm = await Shell.Current.DisplayAlertAsync(
            "Rit verwijderen",
            "Weet je zeker dat je deze rit wilt verwijderen?",
            "Verwijderen",
            "Annuleren");

        if (!confirm)
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            var result = await _workSessionService.DeleteAsync(SessionId);
            if (!result.IsSuccess)
            {
                ErrorMessage = result.Error ?? "Kon rit niet verwijderen.";
                return;
            }

            await Shell.Current.GoToAsync("..");
        });
    }

    private async Task LoadCustomerOptionsAsync()
    {
        CustomerOptions.Clear();
        CustomerOptions.Add(CustomerPickerItem.None);

        var result = await _customerService.GetCustomersAsync();
        if (!result.IsSuccess || result.Value is null)
        {
            return;
        }

        foreach (var customer in result.Value.Where(customer => customer.IsActive))
        {
            CustomerOptions.Add(new CustomerPickerItem(customer.Id, customer.Name));
        }
    }

    private SegmentListItem MapSegment(WorkSessionSegmentDto segment)
    {
        var item = new SegmentListItem
        {
            Id = segment.Id,
            Type = segment.Type,
            StartLocal = ToLocalTime(segment.StartUtc),
            EndLocal = ToLocalTime(segment.EndUtc),
            DistanceKmText = segment.DistanceKm?.ToString("F2", CultureInfo.InvariantCulture) ?? string.Empty,
            SelectedCustomer = ResolveCustomerSelection(segment.CustomerId),
            CustomerName = segment.CustomerName,
            CustomerId = segment.CustomerId,
        };

        return item;
    }

    private CustomerPickerItem ResolveCustomerSelection(Guid? customerId)
    {
        return CustomerOptions.FirstOrDefault(option => option.Id == customerId)
            ?? CustomerPickerItem.None;
    }
    private static void ApplySavedSegment(SegmentListItem segment, WorkSessionSegmentDto savedSegment)
    {
        segment.StartLocal = ToLocalTime(savedSegment.StartUtc);
        segment.EndLocal = ToLocalTime(savedSegment.EndUtc);
        segment.DistanceKmText = savedSegment.DistanceKm?.ToString("F2", CultureInfo.InvariantCulture) ?? string.Empty;
        segment.CustomerName = savedSegment.CustomerName;
        segment.CustomerId = savedSegment.CustomerId;
    }


    private async Task RefreshTripSummaryAsync()
    {
        var result = await _workSessionService.GetWorkSessionDetailsAsync(SessionId);
        if (!result.IsSuccess || result.Value is null)
        {
            return;
        }

        TotalDistanceKm = result.Value.TotalDistanceKm;
        EndLabel = result.Value.EndTimeUtc is { } endUtc
            ? ToLocalTime(endUtc).ToString("dd MMM yyyy HH:mm", DutchCulture)
            : "Bezig";
    }

    private static string TranslateStatus(string status) =>
        status switch
        {
            "InProgress" => "Bezig",
            "Completed" => "Voltooid",
            "Draft" => "Concept",
            _ => status
        };

    private static DateTime ToLocalTime(DateTime utc) =>
        DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToLocalTime();

    private static DateTime ToUtc(DateTime local) =>
        DateTime.SpecifyKind(local, DateTimeKind.Local).ToUniversalTime();

    private async Task ExecuteWithLoadingAsync(Func<Task> action)
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
}

public partial class SegmentListItem : ObservableObject
{
    public Guid Id { get; init; }

    public string Type { get; init; } = string.Empty;

    public bool IsDriving => string.Equals(Type, "Driving", StringComparison.OrdinalIgnoreCase);

    public bool IsStationary => !IsDriving;

    [ObservableProperty]
    public partial DateTime StartLocal { get; set; }

    [ObservableProperty]
    public partial DateTime EndLocal { get; set; }

    public DateTime StartDate
    {
        get => StartLocal.Date;
        set
        {
            if (StartLocal.Date == value.Date)
            {
                return;
            }

            StartLocal = value.Date + StartLocal.TimeOfDay;
        }
    }

    public TimeSpan StartTime
    {
        get => StartLocal.TimeOfDay;
        set
        {
            if (StartLocal.TimeOfDay == value)
            {
                return;
            }

            StartLocal = StartLocal.Date + value;
        }
    }

    public DateTime EndDate
    {
        get => EndLocal.Date;
        set
        {
            if (EndLocal.Date == value.Date)
            {
                return;
            }

            EndLocal = value.Date + EndLocal.TimeOfDay;
        }
    }

    public TimeSpan EndTime
    {
        get => EndLocal.TimeOfDay;
        set
        {
            if (EndLocal.TimeOfDay == value)
            {
                return;
            }

            EndLocal = EndLocal.Date + value;
        }
    }

    partial void OnStartLocalChanged(DateTime value)
    {
        OnPropertyChanged(nameof(StartDate));
        OnPropertyChanged(nameof(StartTime));
    }

    partial void OnEndLocalChanged(DateTime value)
    {
        OnPropertyChanged(nameof(EndDate));
        OnPropertyChanged(nameof(EndTime));
    }

    [ObservableProperty]
    public partial string DistanceKmText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial CustomerPickerItem? SelectedCustomer { get; set; }

    [ObservableProperty]
    public partial string? CustomerName { get; set; }

    [ObservableProperty]
    public partial Guid? CustomerId { get; set; }

    public string Title =>
        IsDriving
            ? "Rijden"
            : string.IsNullOrWhiteSpace(CustomerName)
                ? "Stilstand"
                : $"Klantbezoek · {CustomerName}";

    partial void OnCustomerNameChanged(string? value) => OnPropertyChanged(nameof(Title));

    partial void OnSelectedCustomerChanged(CustomerPickerItem? value)
    {
        CustomerName = value?.Id is null ? null : value.Name;
    }
}

public sealed class CustomerPickerItem
{
    public static CustomerPickerItem None { get; } = new(null, "Geen");

    public CustomerPickerItem(Guid? id, string name)
    {
        Id = id;
        Name = name;
    }

    public Guid? Id { get; }

    public string Name { get; }
}
