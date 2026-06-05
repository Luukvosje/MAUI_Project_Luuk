using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TimeOn.Mobile.Features.Tracking.Models;
using TimeOn.Mobile.Features.Tracking.Services;
using TimeOn.Mobile.Interfaces;

namespace TimeOn.Mobile.Features.Tracking.ViewModels;

public partial class TrackingViewModel : ObservableObject
{
    private readonly IGpsTrackingService _gpsTrackingService;
    private readonly IDevelopmentModeService _developmentModeService;

    [ObservableProperty]
    public partial bool IsTracking { get; set; }

    [ObservableProperty]
    public partial string StatusText { get; set; } = "Tracking is idle.";

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial bool IsDevelopmentModeEnabled { get; set; }

    [ObservableProperty]
    public partial string? GpsPointsJson { get; set; }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool ShowDevelopmentTools =>
        _developmentModeService.IsSupported && IsDevelopmentModeEnabled;

    public TrackingViewModel(
        IGpsTrackingService gpsTrackingService,
        IDevelopmentModeService developmentModeService)
    {
        _gpsTrackingService = gpsTrackingService;
        _developmentModeService = developmentModeService;
        _gpsTrackingService.StateChanged += OnTrackingStateChanged;
        _developmentModeService.Changed += OnDevelopmentModeChanged;
        RefreshDevelopmentMode();
        UpdateFromService();
    }

    public void OnAppearing()
    {
        RefreshDevelopmentMode();
    }

    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task StartAsync()
    {
        ErrorMessage = null;
        OnPropertyChanged(nameof(HasError));

        try
        {
            await _gpsTrackingService.StartAsync();
            UpdateFromService();
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
            OnPropertyChanged(nameof(HasError));
            UpdateFromService();
        }
    }

    [RelayCommand(CanExecute = nameof(CanStop))]
    private async Task StopAsync()
    {
        ErrorMessage = null;
        OnPropertyChanged(nameof(HasError));

        try
        {
            var result = await _gpsTrackingService.StopAsync();
            UpdateFromService();
            StatusText = result.SubmittedToApi
                ? $"Work session saved ({result.GpsPointCount} GPS points)."
                : "Tracking stopped. No GPS points were recorded, so nothing was sent to the server.";
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
            OnPropertyChanged(nameof(HasError));
            UpdateFromService();
        }
    }

    [RelayCommand(CanExecute = nameof(CanSaveImportedWorkSession))]
    private async Task SaveImportedWorkSessionAsync()
    {
        ErrorMessage = null;
        OnPropertyChanged(nameof(HasError));

        try
        {
            var points = GpsPointsClipboardParser.Parse(GpsPointsJson!);
            var response = await _gpsTrackingService.SubmitImportedGpsPointsAsync(points);
            GpsPointsJson = null;
            SaveImportedWorkSessionCommand.NotifyCanExecuteChanged();
            StatusText =
                $"Work session saved ({response.TotalDistanceKm:F2} km, {response.DrivingSegmentCount} driving, {response.StationarySegmentCount} stationary).";
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
            OnPropertyChanged(nameof(HasError));
        }
    }

    partial void OnGpsPointsJsonChanged(string? value)
    {
        SaveImportedWorkSessionCommand.NotifyCanExecuteChanged();
    }

    private bool CanStart() => !IsTracking;

    private bool CanStop() => IsTracking;

    private bool CanSaveImportedWorkSession() =>
        ShowDevelopmentTools &&
        !IsTracking &&
        !string.IsNullOrWhiteSpace(GpsPointsJson);

    private void OnTrackingStateChanged(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(UpdateFromService);
    }

    private void OnDevelopmentModeChanged(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            RefreshDevelopmentMode();
            SaveImportedWorkSessionCommand.NotifyCanExecuteChanged();
        });
    }

    private void RefreshDevelopmentMode()
    {
        IsDevelopmentModeEnabled = _developmentModeService.IsEnabled;
        OnPropertyChanged(nameof(ShowDevelopmentTools));

        if (!ShowDevelopmentTools)
        {
            GpsPointsJson = null;
        }

        StartCommand.NotifyCanExecuteChanged();
        StopCommand.NotifyCanExecuteChanged();
        SaveImportedWorkSessionCommand.NotifyCanExecuteChanged();
    }

    private void UpdateFromService()
    {
        IsTracking = _gpsTrackingService.State == TrackingState.Running;
        if (!IsTracking)
        {
            StatusText = "Tracking is idle.";
        }
        else
        {
            StatusText = $"Tracking active (session {_gpsTrackingService.CurrentSessionId})";
        }

        StartCommand.NotifyCanExecuteChanged();
        StopCommand.NotifyCanExecuteChanged();
        SaveImportedWorkSessionCommand.NotifyCanExecuteChanged();
    }
}
