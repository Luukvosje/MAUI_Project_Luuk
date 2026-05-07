namespace TimeOn.Mobile.Infrastructure.Storage;

public interface IKeyValueStore
{
    Task SetAsync(string key, string value, CancellationToken cancellationToken = default);

    Task<string?> GetAsync(string key, CancellationToken cancellationToken = default);
}
