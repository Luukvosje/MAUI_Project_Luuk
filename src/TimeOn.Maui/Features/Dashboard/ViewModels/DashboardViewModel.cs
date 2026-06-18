using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

using CommunityToolkit.Mvvm.Input;

using TimeOn.Application.Features.Dashboard.DTOs;

using TimeOn.Maui.Features.Dashboard.Services;

using TimeOn.Maui.Features.Tracking.Models;

using TimeOn.Maui.Interfaces;



namespace TimeOn.Maui.Features.Dashboard.ViewModels;



public partial class DashboardViewModel : ObservableObject

{

    private readonly IDashboardSummaryService _dashboardSummaryService;

    private readonly IGpsTrackingService _gpsTrackingService;



    [ObservableProperty]

    public partial string WelcomeMessage { get; set; } = "TimeOn";



    [ObservableProperty]

    public partial bool IsLoading { get; set; }



    [ObservableProperty]

    public partial bool IsRefreshing { get; set; }



    [ObservableProperty]

    public partial string? ErrorMessage { get; set; }



    [ObservableProperty]

    public partial bool IsDayPeriodSelected { get; set; } = true;



    [ObservableProperty]

    public partial bool HasAnyActivity { get; set; }



    [ObservableProperty]

    public partial bool IsTrackingActive { get; set; }



    [ObservableProperty]

    public partial string TrackingStatusText { get; set; } = "Geen actieve registratiesessie";



    [ObservableProperty]

    public partial string DayPeriodLabel { get; set; } = "Vandaag";



    [ObservableProperty]

    public partial string DayDistanceLabel { get; set; } = "0.0 km";



    [ObservableProperty]

    public partial string DayCustomerTimeLabel { get; set; } = "—";



    [ObservableProperty]

    public partial string WeekPeriodLabel { get; set; } = "Deze week";



    [ObservableProperty]

    public partial string WeekDistanceLabel { get; set; } = "0.0 km";



    [ObservableProperty]

    public partial string WeekCustomerTimeLabel { get; set; } = "—";



    public ObservableCollection<DashboardWeekDayItem> WeekDays { get; } = [];



    public bool ShowDayOverview => IsDayPeriodSelected && HasAnyActivity;



    public bool ShowWeekOverview => !IsDayPeriodSelected && HasAnyActivity;



    public bool ShowStartTrackingCard => !IsTrackingActive;



    public bool ShowEmptyState => !IsLoading && string.IsNullOrWhiteSpace(ErrorMessage) && !HasAnyActivity;



    public DashboardViewModel(

        IDashboardSummaryService dashboardSummaryService,

        IGpsTrackingService gpsTrackingService)

    {

        _dashboardSummaryService = dashboardSummaryService;

        _gpsTrackingService = gpsTrackingService;

    }



    public async Task LoadOnAppearAsync()

    {

        UpdateTrackingStatus();



        if (!LoadCommand.IsRunning)

        {

            await LoadCommand.ExecuteAsync(null);

        }

    }



    [RelayCommand]

    private async Task LoadAsync()

    {

        await ExecuteWithLoadingAsync(LoadSummaryAsync, useRefreshIndicator: IsRefreshing);

    }



    [RelayCommand]

    private async Task RefreshAsync()

    {

        IsRefreshing = true;

        try

        {

            await LoadAsync();

        }

        finally

        {

            IsRefreshing = false;

        }

    }



    [RelayCommand]

    private void SelectDayPeriod()

    {

        if (IsDayPeriodSelected)

        {

            return;

        }



        IsDayPeriodSelected = true;

        NotifyPeriodVisibilityChanged();

    }



    [RelayCommand]

    private void SelectWeekPeriod()

    {

        if (!IsDayPeriodSelected)

        {

            return;

        }



        IsDayPeriodSelected = false;

        NotifyPeriodVisibilityChanged();

    }



    partial void OnIsDayPeriodSelectedChanged(bool value) =>

        NotifyPeriodVisibilityChanged();



    partial void OnHasAnyActivityChanged(bool value)

    {

        OnPropertyChanged(nameof(ShowDayOverview));

        OnPropertyChanged(nameof(ShowWeekOverview));

        OnPropertyChanged(nameof(ShowEmptyState));

    }



    private void NotifyPeriodVisibilityChanged()

    {

        OnPropertyChanged(nameof(ShowDayOverview));

        OnPropertyChanged(nameof(ShowWeekOverview));

    }



    private async Task LoadSummaryAsync()

    {

        var result = await _dashboardSummaryService.GetSummaryAsync();

        if (!result.IsSuccess || result.Value is null)

        {

            ErrorMessage = result.Error ?? "Kon dashboardoverzicht niet laden.";

            OnPropertyChanged(nameof(ShowEmptyState));

            return;

        }



        ApplySummary(result.Value);

        UpdateTrackingStatus();

    }



    private void ApplySummary(DashboardSummaryResponseDto summary)

    {

        HasAnyActivity = summary.HasAnyActivity;



        DayPeriodLabel = TranslatePeriodLabel(summary.Day.PeriodLabel);

        DayDistanceLabel = DashboardMetricsFormatter.FormatDistance(summary.Day.TotalDistanceKm);

        DayCustomerTimeLabel = DashboardMetricsFormatter.FormatCustomerTime(summary.Day.CustomerMinutes);



        WeekPeriodLabel = TranslatePeriodLabel(summary.Week.PeriodLabel);

        WeekDistanceLabel = DashboardMetricsFormatter.FormatDistance(summary.Week.TotalDistanceKm);

        WeekCustomerTimeLabel = DashboardMetricsFormatter.FormatCustomerTime(summary.Week.CustomerMinutes);



        WeekDays.Clear();

        foreach (var day in summary.Week.Days)

        {

            WeekDays.Add(new DashboardWeekDayItem(

                DashboardMetricsFormatter.FormatWeekDayLabel(day.Date),

                day.HasActivity

                    ? DashboardMetricsFormatter.FormatDistance(day.TotalDistanceKm)

                    : "—",

                day.HasActivity

                    ? DashboardMetricsFormatter.FormatCustomerTime(day.CustomerMinutes)

                    : "—"));

        }



        OnPropertyChanged(nameof(ShowEmptyState));

        OnPropertyChanged(nameof(ShowDayOverview));

        OnPropertyChanged(nameof(ShowWeekOverview));

    }



    private static string TranslatePeriodLabel(string label) =>

        label switch

        {

            "Today" => "Vandaag",

            "This week" => "Deze week",

            _ => label

        };



    private void UpdateTrackingStatus()

    {

        IsTrackingActive = _gpsTrackingService.State == TrackingState.Running;

        TrackingStatusText = IsTrackingActive

            ? $"Werkdag bezig (sessie {_gpsTrackingService.CurrentSessionId})"

            : "Geen actieve registratiesessie";

        OnPropertyChanged(nameof(ShowStartTrackingCard));

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

            OnPropertyChanged(nameof(ShowEmptyState));

            await action();

        }

        catch (Exception)

        {

            ErrorMessage = "Er is een onverwachte fout opgetreden.";

            OnPropertyChanged(nameof(ShowEmptyState));

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



public sealed record DashboardWeekDayItem(

    string DayLabel,

    string DistanceLabel,

    string CustomerTimeLabel);

