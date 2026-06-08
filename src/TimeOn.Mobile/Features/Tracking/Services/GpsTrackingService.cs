using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TimeOn.Application.Features.WorkSessions.DTOs;
using TimeOn.Domain.Entities;
using TimeOn.Mobile.Features.Tracking.Models;
using TimeOn.Mobile.Interfaces;
using TimeOn.Mobile.Services;

namespace TimeOn.Mobile.Features.Tracking.Services;

public sealed class GpsTrackingService : IGpsTrackingService
{
    private readonly ITrackingGpsStore _gpsStore;
    private readonly IApiService _apiService;
    private readonly IPlatformLocationTracker _platformTracker;
    private readonly ILocalStorageService _localStorageService;
    private readonly INotificationService _notificationService;
    private readonly IGpsNotificationSettingsService _gpsNotificationSettingsService;
    private readonly DrivingStateDetector _drivingStateDetector;
    private readonly ILogger<GpsTrackingService> _logger;
    private readonly SemaphoreSlim _lifecycleLock = new(1, 1);

    private ActiveTrackingSession? _activeSession;

    public GpsTrackingService(
        ITrackingGpsStore gpsStore,
        IApiService apiService,
        IPlatformLocationTracker platformTracker,
        ILocalStorageService localStorageService,
        INotificationService notificationService,
        IGpsNotificationSettingsService gpsNotificationSettingsService,
        DrivingStateDetector drivingStateDetector,
        ILogger<GpsTrackingService> logger)
    {
        _gpsStore = gpsStore;
        _apiService = apiService;
        _platformTracker = platformTracker;
        _localStorageService = localStorageService;
        _notificationService = notificationService;
        _gpsNotificationSettingsService = gpsNotificationSettingsService;
        _drivingStateDetector = drivingStateDetector;
        _logger = logger;
    }

    public TrackingState State { get; private set; } = TrackingState.Idle;

    public Guid? CurrentSessionId => _activeSession?.Id;

    public event EventHandler? StateChanged;

    public async Task StartAsync()
    {
        await _lifecycleLock.WaitAsync();
        try
        {
            if (State == TrackingState.Running)
            {
                return;
            }

            await _gpsStore.InitializeAsync();

            var permission = await RequestLocationPermissionAsync();
            if (permission != PermissionStatus.Granted)
            {
                throw new InvalidOperationException("Location permission is required to start tracking.");
            }

            var userId = await ResolveUserIdAsync();
            if (userId == Guid.Empty)
            {
                throw new InvalidOperationException("You must be logged in to start tracking.");
            }

            _activeSession = await _gpsStore.GetActiveSessionAsync(userId);
            if (_activeSession is null)
            {
                _activeSession = new ActiveTrackingSession(
                    Guid.NewGuid(),
                    userId, 
                    DateTime.UtcNow);
                await _gpsStore.SaveActiveSessionAsync(_activeSession);
                _logger.LogInformation(
                    "Tracking session {SessionId} started at {StartTimeUtc:O}",
                    _activeSession.Id,
                    _activeSession.StartTimeUtc);
            }

            _drivingStateDetector.Reset();
            SetState(TrackingState.Running);
            await _platformTracker.StartAsync(IngestAsync);
        }
        finally
        {
            _lifecycleLock.Release();
        }
    }

    public async Task<StopTrackingResult> StopAsync()
    {
        await _lifecycleLock.WaitAsync();
        try
        {
            if (State != TrackingState.Running || _activeSession is null)
            {
                return new StopTrackingResult(SubmittedToApi: false, GpsPointCount: 0);
            }

            var session = _activeSession;
            await _platformTracker.StopAsync();

            var gpsPoints = await _gpsStore.GetPointsAsync(session.Id);
            _logger.LogInformation(
                "Stopping session {SessionId}: {GpsPointCount} GPS points collected",
                session.Id,
                gpsPoints.Count);

            if (gpsPoints.Count == 0)
            {
                _logger.LogInformation(
                    "Session {SessionId} discarded without API submission (no GPS points).",
                    session.Id);
                await _gpsStore.ClearActiveSessionAsync(session.UserId);
                _activeSession = null;
                _drivingStateDetector.Reset();
                SetState(TrackingState.Idle);
                return new StopTrackingResult(SubmittedToApi: false, GpsPointCount: 0);
            }

            var endTimeUtc = DateTime.UtcNow;
            var dtos = gpsPoints.Select(point => new GpsPointDto(
                point.Location.Latitude,
                point.Location.Longitude,
                point.RecordedAtUtc)).ToList();

            var response = await CompleteWorkSessionAsync(
                session.Id,
                session.StartTimeUtc,
                endTimeUtc,
                dtos);

            await _gpsStore.DeletePointsAsync(session.Id);
            await _gpsStore.ClearActiveSessionAsync(session.UserId);

            _activeSession = null;
            _drivingStateDetector.Reset();
            SetState(TrackingState.Idle);

            LogWorkSessionSubmitted(response);
            return new StopTrackingResult(SubmittedToApi: true, GpsPointCount: gpsPoints.Count);
        }
        finally
        {
            _lifecycleLock.Release();
        }
    }

    public async Task ResumeIfActiveAsync()
    {
        await _lifecycleLock.WaitAsync();
        try
        {
            if (State == TrackingState.Running)
            {
                return;
            }

            await _gpsStore.InitializeAsync();

            var userId = await ResolveUserIdAsync();
            _activeSession = await _gpsStore.GetActiveSessionAsync(userId);
            if (_activeSession is null)
            {
                return;
            }

            var permission = await RequestLocationPermissionAsync();
            if (permission != PermissionStatus.Granted)
            {
                _logger.LogWarning(
                    "Active tracking session {SessionId} found but location permission was not granted.",
                    _activeSession.Id);
                return;
            }

            _drivingStateDetector.Reset();
            SetState(TrackingState.Running);
            await _platformTracker.StartAsync(IngestAsync);
            _logger.LogInformation("Resumed tracking for session {SessionId}", _activeSession.Id);
        }
        finally
        {
            _lifecycleLock.Release();
        }
    }

    public async Task<CompleteWorkSessionResponse> SubmitImportedGpsPointsAsync(
        IReadOnlyList<GpsPointDto> gpsPoints)
    {
        if (gpsPoints.Count == 0)
        {
            throw new InvalidOperationException("At least one GPS point is required.");
        }

        await _lifecycleLock.WaitAsync();
        try
        {
            if (State == TrackingState.Running)
            {
                throw new InvalidOperationException("Stop tracking before submitting imported GPS points.");
            }

            var userId = await ResolveUserIdAsync();
            if (userId == Guid.Empty)
            {
                throw new InvalidOperationException("You must be logged in to save a work session.");
            }

            var ordered = gpsPoints.OrderBy(point => point.RecordedAtUtc).ToList();
            var sessionId = Guid.NewGuid();
            var response = await CompleteWorkSessionAsync(
                sessionId,
                ordered[0].RecordedAtUtc,
                ordered[^1].RecordedAtUtc,
                ordered);

            LogWorkSessionSubmitted(response);
            return response;
        }
        finally
        {
            _lifecycleLock.Release();
        }
    }

    internal async Task IngestAsync(LocationReading reading)
    {
        try
        {
            if (_activeSession is null || State != TrackingState.Running)
            {
                _logger.LogDebug(
                    "GPS reading ignored (State={State}, HasActiveSession={HasSession})",
                    State,
                    _activeSession is not null);
                return;
            }

            await HandleDrivingStateTransitionAsync(reading);

            var lastPoint = await _gpsStore.GetLastPointAsync(_activeSession.Id);
            var rejectReason = GpsSampleEvaluator.GetRejectReason(lastPoint, reading);
            if (rejectReason is not null)
            {
                _logger.LogInformation(
                    "GPS reading skipped for session {SessionId}: {Reason} (lat={Latitude:F6}, lon={Longitude:F6}, accuracy={Accuracy:F1}m)",
                    _activeSession.Id,
                    rejectReason,
                    reading.Latitude,
                    reading.Longitude,
                    reading.Accuracy);
                return;
            }

            var point = GpsPoint.Create(
                reading.Latitude,
                reading.Longitude,
                reading.TimestampUtc);

            await _gpsStore.AddPointAsync(_activeSession.Id, point);
            _logger.LogInformation(
                "GpsPoint created for session {SessionId}: {Latitude:F6}, {Longitude:F6} at {RecordedAtUtc:O} (accuracy={Accuracy:F1}m, isFirst={IsFirst})",
                _activeSession.Id,
                point.Location.Latitude,
                point.Location.Longitude,
                point.RecordedAtUtc,
                reading.Accuracy,
                lastPoint is null);

            if (_gpsNotificationSettingsService.IsEnabled)
            {
                var locationText =
                    $"{point.Location.Latitude:F6}, {point.Location.Longitude:F6}";
                await _notificationService.ShowLocalNotificationAsync(
                    "Location saved",
                    $"Saved at {locationText}");
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to ingest GPS reading (lat={Latitude:F6}, lon={Longitude:F6})",
                reading.Latitude,
                reading.Longitude);
        }
    }

    private async Task HandleDrivingStateTransitionAsync(LocationReading reading)
    {
        var (transition, effectiveSpeedKmh) = _drivingStateDetector.Evaluate(reading);

        _logger.LogDebug(
            "Driving state sample for session {SessionId}: reported={ReportedSpeedKmh:F1} km/h, effective={EffectiveSpeedKmh:F1} km/h, transition={Transition}",
            _activeSession!.Id,
            reading.Speed * 3.6,
            effectiveSpeedKmh,
            transition);

        switch (transition)
        {
            case DrivingStateTransition.StartedDriving:
                _logger.LogInformation(
                    "Driving detected for session {SessionId} at {SpeedKmh:F1} km/h",
                    _activeSession.Id,
                    effectiveSpeedKmh);
                await _notificationService.ShowLocalNotificationAsync(
                    "Driving detected",
                    "Distance tracking is active again.");
                break;
            case DrivingStateTransition.Stopped:
                _logger.LogInformation(
                    "Stop detected for session {SessionId} at {SpeedKmh:F1} km/h",
                    _activeSession.Id,
                    effectiveSpeedKmh);
                await _notificationService.ShowLocalNotificationAsync(
                    "Stop detected",
                    "You appear to be stationary.");
                break;
        }
    }

    private async Task<CompleteWorkSessionResponse> CompleteWorkSessionAsync(
        Guid sessionId,
        DateTime startTimeUtc,
        DateTime endTimeUtc,
        IReadOnlyList<GpsPointDto> gpsPoints)
    {
        var request = new CompleteWorkSessionRequest(
            sessionId,
            startTimeUtc,
            endTimeUtc,
            gpsPoints);

        var response = await _apiService.PostAsync<CompleteWorkSessionRequest, CompleteWorkSessionResponse>(
            "api/worksessions/complete",
            request);

        if (response is null)
        {
            throw new InvalidOperationException(
                _apiService.LastError ?? "Failed to submit work session to the API.");
        }

        return response;
    }

    private void LogWorkSessionSubmitted(CompleteWorkSessionResponse response)
    {
        _logger.LogInformation(
            "WorkSession {SessionId} submitted: {TotalDistanceKm:F2} km, {DrivingCount} driving, {StationaryCount} stationary segments",
            response.Id,
            response.TotalDistanceKm,
            response.DrivingSegmentCount,
            response.StationarySegmentCount);
    }

    private void SetState(TrackingState state)
    {
        if (State == state)
        {
            return;
        }

        State = state;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private static async Task<PermissionStatus> RequestLocationPermissionAsync()
    {
        var whenInUse = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (whenInUse != PermissionStatus.Granted)
        {
            whenInUse = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        if (whenInUse != PermissionStatus.Granted)
        {
            return whenInUse;
        }

#if ANDROID
        var always = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
        if (always != PermissionStatus.Granted)
        {
            always = await Permissions.RequestAsync<Permissions.LocationAlways>();
        }

        return always;
#else
        return whenInUse;
#endif
    }

    private async Task<Guid> ResolveUserIdAsync()
    {
        var token = await _localStorageService.GetAsync<string>(AuthenticationService.AuthTokenKey);
        if (string.IsNullOrWhiteSpace(token))
        {
            return Guid.Empty;
        }

        return ParseUserIdFromToken(token);
    }

    private static Guid ParseUserIdFromToken(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length < 2)
            {
                return Guid.Empty;
            }

            var payload = parts[1]
                .Replace('-', '+')
                .Replace('_', '/');

            switch (payload.Length % 4)
            {
                case 2:
                    payload += "==";
                    break;
                case 3:
                    payload += "=";
                    break;
            }

            var jsonBytes = Convert.FromBase64String(payload);
            var json = Encoding.UTF8.GetString(jsonBytes);
            using var document = JsonDocument.Parse(json);

            foreach (var propertyName in new[]
                     {
                         "sub",
                         "nameid",
                         "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
                     })
            {
                if (!document.RootElement.TryGetProperty(propertyName, out var claimElement))
                {
                    continue;
                }

                var value = claimElement.GetString();
                if (!string.IsNullOrWhiteSpace(value) && Guid.TryParse(value, out var userId))
                {
                    return userId;
                }
            }
        }
        catch (Exception)
        {
            return Guid.Empty;
        }

        return Guid.Empty;
    }
}
