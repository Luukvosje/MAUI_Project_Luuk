using TimeOn.Mobile.Core.Interfaces;
using TimeOn.Mobile.Core.Models;

namespace TimeOn.Mobile.Infrastructure.Device;

public sealed class TrackingService : ITrackingService
{
    private readonly ILocationService locationService;
    private readonly ITripRepository tripRepository;
    private readonly INotificationService notificationService;
    private Trip? activeTrip;

    public TrackingService(
        ILocationService locationService,
        ITripRepository tripRepository,
        INotificationService notificationService)
    {
        this.locationService = locationService;
        this.tripRepository = tripRepository;
        this.notificationService = notificationService;
    }

    public bool IsTracking => activeTrip is not null;

    public async Task StartWorkDayAsync(CancellationToken cancellationToken = default)
    {
        if (activeTrip is not null)
        {
            return;
        }

        _ = await locationService.GetCurrentLocationAsync(cancellationToken);
        activeTrip = new Trip { StartTime = DateTimeOffset.UtcNow };
        await notificationService.ShowAsync("Tracking started", "Your workday tracking is active.", cancellationToken);
    }

    public async Task StopWorkDayAsync(CancellationToken cancellationToken = default)
    {
        if (activeTrip is null)
        {
            return;
        }

        activeTrip.EndTime = DateTimeOffset.UtcNow;
        await tripRepository.SaveTripAsync(activeTrip, cancellationToken);
        activeTrip = null;
        await notificationService.ShowAsync("Tracking stopped", "Your trip is saved.", cancellationToken);
    }
}
