using System.Net.Http.Json;
using System.Text.Json;
using TimeOn.Maui.Interfaces;

namespace TimeOn.Maui.Services;

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
                $"Kan de API niet bereiken op {baseUrl}." +
                (string.IsNullOrWhiteSpace(detail) ? string.Empty : $" ({detail})") +
                " Start TimeOn.Api op je pc (luistert op http://0.0.0.0:5000)." +
                " De emulator gebruikt http://10.0.2.2:5000 om de hostmachine te bereiken." +
                " Fysiek apparaat op Wi-Fi: stel het LAN-IP van je pc in via appsettings.android.json (bijv. http://192.168.x.x:5000/).";

            return message;
        }

        return $"Kan de API niet bereiken op {baseUrl}." +
               (string.IsNullOrWhiteSpace(detail) ? string.Empty : $" ({detail})") +
               " Start TimeOn.Api op deze machine.";
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
            System.Net.HttpStatusCode.BadRequest => "Het verzoek was ongeldig.",
            System.Net.HttpStatusCode.Unauthorized => "Je sessie is verlopen. Log opnieuw in.",
            System.Net.HttpStatusCode.NotFound => "De gevraagde resource is niet gevonden.",
            System.Net.HttpStatusCode.InternalServerError => "Er is een serverfout opgetreden.",
            _ => $"Verzoek mislukt ({(int)response.StatusCode})."
        };
    }

    private sealed class ApiErrorResponse
    {
        public string? Error { get; init; }
    }
}
