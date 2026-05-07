namespace TimeOn.Mobile.Core.Interfaces;

public interface ITrackingService
{
    bool IsTracking { get; }

    Task StartWorkDayAsync(CancellationToken cancellationToken = default);

    Task StopWorkDayAsync(CancellationToken cancellationToken = default);
}
