using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TimeOn.Application.Features.WorkSessions.DTOs;
using TimeOn.Application.Features.WorkSessions.Services;
using TimeOn.Maui.Features.Trips.Views;

namespace TimeOn.Maui.Features.Trips.ViewModels;

public partial class TripsViewModel : ObservableObject
{
    private readonly IWorkSessionService _workSessionService;

    public ObservableCollection<TripListItem> Sessions { get; } = [];

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial bool IsRefreshing { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    public TripsViewModel(IWorkSessionService workSessionService)
    {
        _workSessionService = workSessionService;
    }

    public async Task LoadOnAppearAsync()
    {
        if (!LoadSessionsCommand.IsRunning)
        {
            await LoadSessionsCommand.ExecuteAsync(null);
        }
    }

    [RelayCommand]
    private async Task LoadSessionsAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            var result = await _workSessionService.GetAllAsync();
            if (!result.IsSuccess || result.Value is null)
            {
                ErrorMessage = result.Error ?? "Could not load trips.";
                return;
            }

            Sessions.Clear();
            foreach (var session in result.Value)
            {
                Sessions.Add(MapSession(session));
            }
        }, useRefreshIndicator: IsRefreshing);
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        try
        {
            await LoadSessionsAsync();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task OpenTripAsync(TripListItem session)
    {
        await Shell.Current.GoToAsync(nameof(TripDetailPage), new Dictionary<string, object>
        {
            ["sessionId"] = session.Id
        });
    }

    private static TripListItem MapSession(WorkSessionListItemDto session)
    {
        var start = session.StartTimeUtc.ToLocalTime();
        var end = session.EndTimeUtc?.ToLocalTime();
        var endLabel = end is null
            ? "In progress"
            : $"Ended {end:dd MMM yyyy HH:mm}";

        return new TripListItem(
            session.Id,
            start.ToString("dd MMM yyyy HH:mm"),
            endLabel,
            session.Status,
            $"{session.TotalDistanceKm:F2} km");
    }

    private async Task ExecuteWithLoadingAsync(Func<Task> action, bool useRefreshIndicator)
    {
        try
        {
            if (!useRefreshIndicator)
            {
                IsLoading = true;
            }

            ErrorMessage = null;
            await action();
        }
        catch (Exception)
        {
            ErrorMessage = "An unexpected error occurred.";
        }
        finally
        {
            if (!useRefreshIndicator)
            {
                IsLoading = false;
            }
        }
    }
}

public sealed record TripListItem(Guid Id, string StartLabel, string EndLabel, string Status, string DistanceLabel);
