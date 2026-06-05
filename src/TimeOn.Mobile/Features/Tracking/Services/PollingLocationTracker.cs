using Microsoft.Extensions.Logging;
using TimeOn.Domain.Constants;
using TimeOn.Mobile.Features.Tracking.Models;

namespace TimeOn.Mobile.Features.Tracking.Services;

public sealed class PollingLocationTracker : IPlatformLocationTracker
{
    private readonly ILogger<PollingLocationTracker> _logger;
    private CancellationTokenSource? _cancellationTokenSource;
    private Func<LocationReading, Task>? _onReading;

    public PollingLocationTracker(ILogger<PollingLocationTracker> logger)
    {
        _logger = logger;
    }

    public Task StartAsync(Func<LocationReading, Task> onReading)
    {
        _onReading = onReading;
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        _logger.LogInformation("Polling location tracker started.");
        _ = RunPollingLoopAsync(_cancellationTokenSource.Token);
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        _logger.LogInformation("Polling location tracker stopped.");
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = null;
        _onReading = null;
        return Task.CompletedTask;
    }

    private async Task RunPollingLoopAsync(CancellationToken cancellationToken)
    {
        var lastSpeedKmh = 0d;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
#if WINDOWS
                var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(30));
#else
                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(15));
#endif
                var location = await Geolocation.Default.GetLocationAsync(request, cancellationToken);
                if (location is null)
                {
                    _logger.LogDebug("Geolocation poll returned no location.");
                }
                else if (_onReading is null)
                {
                    _logger.LogDebug(
                        "Geolocation poll received lat={Latitude:F6}, lon={Longitude:F6} but tracking is not listening.",
                        location.Latitude,
                        location.Longitude);
                }
                else
                {
                    var accuracy = location.Accuracy ?? LocationReading.UnknownAccuracy;
                    var reading = new LocationReading(
                        location.Latitude,
                        location.Longitude,
                        accuracy,
                        location.Speed ?? 0d,
                        location.Timestamp.UtcDateTime);

                    _logger.LogDebug(
                        "Geolocation poll: lat={Latitude:F6}, lon={Longitude:F6}, accuracy={Accuracy:F1}m, speed={SpeedKmh:F1} km/h",
                        reading.Latitude,
                        reading.Longitude,
                        reading.Accuracy,
                        reading.Speed * 3.6);

                    await _onReading(reading);

                    lastSpeedKmh = (location.Speed ?? 0d) * 3.6;
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Failed to poll device location.");
            }

            var delaySeconds = lastSpeedKmh > TrackingOptions.FastSpeedKmh
                ? TrackingOptions.FastIntervalSeconds
                : TrackingOptions.DefaultIntervalSeconds;

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}
