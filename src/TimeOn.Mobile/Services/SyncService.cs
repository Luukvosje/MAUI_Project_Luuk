using Microsoft.Extensions.Logging;
using TimeOn.Application.Features.Customers.DTOs;
using TimeOn.Mobile.Caching;
using TimeOn.Mobile.Interfaces;
using TimeOn.Mobile.Sync;

namespace TimeOn.Mobile.Services;

public sealed class SyncService : ISyncService
{
    private const string CustomersCacheKey = "customers-cache";
    private const string CustomersEndpoint = "api/customers";

    private readonly ISyncQueue _syncQueue;
    private readonly IApiService _apiService;
    private readonly ICacheStore _cacheStore;
    private readonly ILogger<SyncService> _logger;

    public SyncService(
        ISyncQueue syncQueue,
        IApiService apiService,
        ICacheStore cacheStore,
        ILogger<SyncService> logger)
    {
        _syncQueue = syncQueue;
        _apiService = apiService;
        _cacheStore = cacheStore;
        _logger = logger;
    }

    public bool HasPendingChanges => _syncQueue.Count > 0;

    public async Task SyncPendingChangesAsync()
    {
        await PullLatestDataAsync();
        await DrainQueueAsync();
    }

    private async Task DrainQueueAsync()
    {
        var pendingQueueItems = await _syncQueue.SnapshotAsync();
        foreach (var _ in pendingQueueItems)
        {
            await _syncQueue.DequeueAsync();
        }
    }

    private async Task PullLatestDataAsync()
    {
        try
        {
            var customers = await _apiService.GetAsync<IReadOnlyList<CustomerDto>>(CustomersEndpoint) ?? [];
            await _cacheStore.SetAsync(CustomersCacheKey, customers);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Customer startup sync failed; continuing with cached data.");
        }
    }
}
