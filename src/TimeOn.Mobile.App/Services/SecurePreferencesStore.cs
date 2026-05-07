using TimeOn.Mobile.Infrastructure.Storage;

namespace TimeOn.Mobile.App.Services;

public sealed class SecurePreferencesStore : IKeyValueStore
{
    public async Task SetAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        await SecureStorage.Default.SetAsync(key, value);
    }

    public async Task<string?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        return await SecureStorage.Default.GetAsync(key);
    }
}
