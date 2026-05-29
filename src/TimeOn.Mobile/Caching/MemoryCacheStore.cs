namespace TimeOn.Mobile.Caching;

public sealed class MemoryCacheStore : ICacheStore
{
    private readonly Dictionary<string, object> _cache = new(StringComparer.Ordinal);

    public Task<T?> GetAsync<T>(string key)
    {
        
        if (_cache.TryGetValue(key, out var value) && value is T typed)
        {
            return Task.FromResult<T?>(typed);
        }

        return Task.FromResult<T?>(default);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)
    {
        _ = ttl;
        
        _cache[key] = value!;
        return Task.CompletedTask;
    }

    public Task InvalidateAsync(string key)
    {
        
        _cache.Remove(key);
        return Task.CompletedTask;
    }
}
