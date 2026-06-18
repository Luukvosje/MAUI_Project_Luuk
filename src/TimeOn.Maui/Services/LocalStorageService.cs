using System.Text.Json;
using TimeOn.Maui.Interfaces;

namespace TimeOn.Maui.Services;

public sealed class LocalStorageService : ILocalStorageService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public Task<T?> GetAsync<T>(string key)
    {
        if (!Preferences.ContainsKey(key))
        {
            return Task.FromResult<T?>(default);
        }

        var value = Preferences.Get(key, string.Empty);
        if (string.IsNullOrEmpty(value))
        {
            return Task.FromResult<T?>(default);
        }

        if (typeof(T) == typeof(string))
        {
            return Task.FromResult((T?)(object)value);
        }

        var deserialized = JsonSerializer.Deserialize<T>(value, SerializerOptions);
        return Task.FromResult(deserialized);
    }

    public Task SetAsync<T>(string key, T value)
    {
        if (value is string stringValue)
        {
            Preferences.Set(key, stringValue);
            return Task.CompletedTask;
        }

        var json = JsonSerializer.Serialize(value, SerializerOptions);
        Preferences.Set(key, json);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        Preferences.Remove(key);
        return Task.CompletedTask;
    }
}
