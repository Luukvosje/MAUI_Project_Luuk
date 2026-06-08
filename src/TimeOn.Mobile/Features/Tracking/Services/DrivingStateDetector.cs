using TimeOn.Domain.Constants;
using TimeOn.Mobile.Features.Tracking.Models;
using TimeOn.Domain.ValueObjects;

namespace TimeOn.Mobile.Features.Tracking.Services;

public sealed class DrivingStateDetector
{
    private const double StoppedSpeedKmh = 5;
    private const double UnreliableReportedSpeedKmh = 3;

    private static readonly TimeSpan DrivingConfirmDuration = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan StoppedConfirmDuration = TrackingConstants.MinimumStopDurationMinutes;
    private static readonly TimeSpan NotificationCooldown = TimeSpan.FromMinutes(5);

    private DrivingMotionState _state = DrivingMotionState.Unknown;
    private DateTime? _candidateSinceUtc;
    private DateTime _lastNotificationUtc = DateTime.MinValue;
    private LocationReading? _lastReading;

    public (DrivingStateTransition Transition, double EffectiveSpeedKmh) Evaluate(LocationReading reading)
    {
        var speedKmh = GetEffectiveSpeedKmh(reading);
        _lastReading = reading;

        if (speedKmh >= TrackingConstants.MinRidingSpeedKph)
        {
            return (EvaluateTowardDriving(), speedKmh);
        }

        if (speedKmh <= StoppedSpeedKmh)
        {
            return (EvaluateTowardStopped(), speedKmh);
        }

        _candidateSinceUtc = null;
        return (DrivingStateTransition.None, speedKmh);
    }

    public void Reset()
    {
        _state = DrivingMotionState.Unknown;
        _candidateSinceUtc = null;
        _lastNotificationUtc = DateTime.MinValue;
        _lastReading = null;
    }

    private double GetEffectiveSpeedKmh(LocationReading reading)
    {
        var reportedKmh = Math.Max(0, reading.Speed * 3.6);
        if (_lastReading is null)
        {
            return reportedKmh;
        }

        var derivedKmh = ComputeDerivedSpeedKmh(_lastReading.Value, reading);
        if (reportedKmh <= UnreliableReportedSpeedKmh)
        {
            return derivedKmh;
        }

        return Math.Max(reportedKmh, derivedKmh);
    }

    private static double ComputeDerivedSpeedKmh(LocationReading previous, LocationReading current)
    {
        var elapsed = current.TimestampUtc - previous.TimestampUtc;
        if (elapsed.TotalSeconds <= 0)
        {
            return 0;
        }

        var from = Coordinate.Create(previous.Latitude, previous.Longitude);
        var to = Coordinate.Create(current.Latitude, current.Longitude);
        var meters = from.DistanceTo(to);

        return meters / elapsed.TotalSeconds * 3.6;
    }

    private DrivingStateTransition EvaluateTowardDriving()
    {
        if (_state == DrivingMotionState.Driving)
        {
            _candidateSinceUtc = null;
            return DrivingStateTransition.None;
        }

        var now = DateTime.UtcNow;
        _candidateSinceUtc ??= now;
        if (now - _candidateSinceUtc.Value < DrivingConfirmDuration)
        {
            return DrivingStateTransition.None;
        }

        var previousState = _state;
        _state = DrivingMotionState.Driving;
        _candidateSinceUtc = null;

        if (previousState != DrivingMotionState.Stopped || !CanNotify(now))
        {
            return DrivingStateTransition.None;
        }

        _lastNotificationUtc = now;
        return DrivingStateTransition.StartedDriving;
    }

    private DrivingStateTransition EvaluateTowardStopped()
    {
        if (_state == DrivingMotionState.Stopped)
        {
            _candidateSinceUtc = null;
            return DrivingStateTransition.None;
        }

        var now = DateTime.UtcNow;
        _candidateSinceUtc ??= now;
        if (now - _candidateSinceUtc.Value < StoppedConfirmDuration)
        {
            return DrivingStateTransition.None;
        }

        var previousState = _state;
        _state = DrivingMotionState.Stopped;
        _candidateSinceUtc = null;

        if (previousState != DrivingMotionState.Driving || !CanNotify(now))
        {
            return DrivingStateTransition.None;
        }

        _lastNotificationUtc = now;
        return DrivingStateTransition.Stopped;
    }

    private bool CanNotify(DateTime timestampUtc) =>
        timestampUtc - _lastNotificationUtc >= NotificationCooldown;

    private enum DrivingMotionState
    {
        Unknown,
        Stopped,
        Driving
    }
}
