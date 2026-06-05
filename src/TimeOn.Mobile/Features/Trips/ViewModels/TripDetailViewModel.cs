using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TimeOn.Application.Features.WorkSessions.DTOs;
using TimeOn.Application.Features.WorkSessions.Services;

namespace TimeOn.Mobile.Features.Trips.ViewModels;

public partial class TripDetailViewModel : ObservableObject
{
    private readonly IWorkSessionService _workSessionService;

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

    public string Title { get; private set; } = "Trip";

    public TripDetailViewModel(IWorkSessionService workSessionService)
    {
        _workSessionService = workSessionService;
    }

    public async Task LoadOnAppearAsync()
    {
        if (SessionId == Guid.Empty)
        {
            ErrorMessage = "Trip id is missing.";
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
            var result = await _workSessionService.GetWorkSessionDetailsAsync(SessionId);
            if (!result.IsSuccess || result.Value is null)
            {
                ErrorMessage = result.Error ?? "Could not load trip.";
                return;
            }

            var trip = result.Value;
            Status = trip.Status;
            StartLabel = trip.StartTimeUtc.ToLocalTime().ToString("dd MMM yyyy HH:mm");
            EndLabel = trip.EndTimeUtc?.ToLocalTime().ToString("dd MMM yyyy HH:mm") ?? "In progress";
            TotalDistanceKm = trip.TotalDistanceKm;
            Title = $"Trip {trip.StartTimeUtc.ToLocalTime():dd MMM yyyy}";

            Segments.Clear();
            foreach (var segment in trip.Segments)
            {
                Segments.Add(MapSegment(segment));
            }

            OnPropertyChanged(nameof(Title));
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
            "Delete trip",
            "Are you sure you want to delete this trip?",
            "Delete",
            "Cancel");

        if (!confirm)
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            var result = await _workSessionService.DeleteAsync(SessionId);
            if (!result.IsSuccess)
            {
                ErrorMessage = result.Error ?? "Could not delete trip.";
                return;
            }

            await Shell.Current.GoToAsync("..");
        });
    }

    private static SegmentListItem MapSegment(WorkSessionSegmentDto segment)
    {
        var timeRange =
            $"{segment.StartUtc.ToLocalTime():dd MMM HH:mm} - {segment.EndUtc.ToLocalTime():HH:mm}";

        if (string.Equals(segment.Type, "Driving", StringComparison.OrdinalIgnoreCase))
        {
            return new SegmentListItem(
                "Driving",
                $"{timeRange}\n{segment.DurationMinutes} min · {segment.DistanceKm:F2} km");
        }

        var visitLabel = segment.IsCustomerVisit ? "Customer visit" : "Stationary";
        var location = segment.CenterLatitude is not null && segment.CenterLongitude is not null
            ? $"\n{segment.CenterLatitude:F5}, {segment.CenterLongitude:F5}"
            : string.Empty;

        return new SegmentListItem(
            visitLabel,
            $"{timeRange}\n{segment.DurationMinutes} min{location}");
    }

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
            ErrorMessage = "An unexpected error occurred.";
        }
        finally
        {
            IsLoading = false;
        }
    }
}

public sealed record SegmentListItem(string Title, string Details);
