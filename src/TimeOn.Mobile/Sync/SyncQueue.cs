using System.Text.Json;
using TimeOn.Mobile.Interfaces;

namespace TimeOn.Mobile.Sync;

public sealed class SyncQueue : ISyncQueue
{
    private const string QueueStorageKey = "sync_queue_payloads";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly Queue<object> _queue = new();
    private readonly ILocalStorageService _localStorageService;
    private bool _isLoaded;

    public SyncQueue(ILocalStorageService localStorageService)
    {
        _localStorageService = localStorageService;
    }

    public int Count => _queue.Count;

    public async Task EnqueueAsync(object payload)
    {
        await EnsureLoadedAsync();
        _queue.Enqueue(payload);
        await PersistAsync();
    }

    public async Task<object?> DequeueAsync()
    {
        await EnsureLoadedAsync();
        if (_queue.Count == 0)
        {
            return null;
        }

        var payload = _queue.Dequeue();
        await PersistAsync();
        return payload;
    }

    public async Task<IReadOnlyList<object>> SnapshotAsync()
    {
        await EnsureLoadedAsync();
        return _queue.ToList();
    }

    private async Task EnsureLoadedAsync()
    {
        if (_isLoaded)
        {
            return;
        }

        var serializedPayloads = await _localStorageService.GetAsync<List<string>>(QueueStorageKey) ?? [];
        foreach (var serializedPayload in serializedPayloads)
        {
            var payload = JsonSerializer.Deserialize<JsonElement>(serializedPayload, SerializerOptions);
            _queue.Enqueue(payload);
        }

        _isLoaded = true;
    }

    private Task PersistAsync()
    {
        var payloads = _queue
            .Select(payload => JsonSerializer.Serialize(payload, SerializerOptions))
            .ToList();
        return _localStorageService.SetAsync(QueueStorageKey, payloads);
    }
}
