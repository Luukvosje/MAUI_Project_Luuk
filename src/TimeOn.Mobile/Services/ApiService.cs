using System.Net.Http.Json;
using System.Text.Json;
using TimeOn.Mobile.Interfaces;

namespace TimeOn.Mobile.Services;

public sealed class ApiService : IApiService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public string? LastError { get; private set; }

    public async Task<TResponse?> GetAsync<TResponse>(string endpoint)
    {
        return await SendAsync(async () =>
        {
            var response = await _httpClient.GetAsync(endpoint);
            return await ReadSuccessResponseAsync<TResponse>(response);
        });
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest request)
    {
        return await SendAsync(async () =>
        {
            var response = await _httpClient.PostAsJsonAsync(endpoint, request);
            return await ReadSuccessResponseAsync<TResponse>(response);
        });
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest request)
    {
        return await SendAsync(async () =>
        {
            var response = await _httpClient.PutAsJsonAsync(endpoint, request);
            return await ReadSuccessResponseAsync<TResponse>(response);
        });
    }

    public async Task DeleteAsync(string endpoint)
    {
        await SendAsync(async () =>
        {
            var response = await _httpClient.DeleteAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                LastError = await ReadErrorMessageAsync(response);
            }

            return true;
        });
    }

    private async Task<T?> SendAsync<T>(Func<Task<T?>> send)
    {
        LastError = null;

        try
        {
            return await send();
        }
        catch (HttpRequestException ex)
        {
            LastError = BuildConnectionErrorMessage(ex);
            return default;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            LastError = BuildConnectionErrorMessage(ex);
            return default;
        }
        catch (TaskCanceledException)
        {
            LastError = BuildConnectionErrorMessage(null);
            return default;
        }
    }

    private string BuildConnectionErrorMessage(Exception? ex)
    {
        var baseUrl = _httpClient.BaseAddress?.ToString().TrimEnd('/') ?? "unknown";
        var detail = ex?.InnerException?.Message ?? ex?.Message;

        if (OperatingSystem.IsAndroid())
        {
            var message =
                $"Cannot reach the API at {baseUrl}." +
                (string.IsNullOrWhiteSpace(detail) ? string.Empty : $" ({detail})") +
                " Start TimeOn.Api on your PC (listening on http://0.0.0.0:5000)." +
                " The emulator uses http://10.0.2.2:5000 to reach the host machine." +
                " Physical device on Wi-Fi: set your PC LAN IP in appsettings.android.json (e.g. http://192.168.x.x:5000/).";

            return message;
        }

        return $"Cannot reach the API at {baseUrl}." +
               (string.IsNullOrWhiteSpace(detail) ? string.Empty : $" ({detail})") +
               " Start TimeOn.Api on this machine.";
    }

    private async Task<TResponse?> ReadSuccessResponseAsync<TResponse>(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            LastError = await ReadErrorMessageAsync(response);
            return default;
        }

        return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions);
    }

    private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response)
    {
        try
        {
            var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions);
            if (!string.IsNullOrWhiteSpace(body?.Error))
            {
                return body.Error;
            }
        }
        catch (JsonException)
        {
            // Fall back to status-based message.
        }

        return response.StatusCode switch
        {
            System.Net.HttpStatusCode.BadRequest => "The request was invalid.",
            System.Net.HttpStatusCode.Unauthorized => "Your session has expired. Please sign in again.",
            System.Net.HttpStatusCode.NotFound => "The requested resource was not found.",
            System.Net.HttpStatusCode.InternalServerError => "A server error occurred.",
            _ => $"Request failed ({(int)response.StatusCode})."
        };
    }

    private sealed class ApiErrorResponse
    {
        public string? Error { get; init; }
    }
}
