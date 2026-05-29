namespace TimeOn.Mobile.Caching;

public interface ICacheStore
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? ttl = null);
    Task InvalidateAsync(string key);
}
