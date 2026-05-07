using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace TimeOn.Mobile.Infrastructure.Api;

public sealed class ApiClient
{
    private readonly HttpClient httpClient;
    private readonly JwtTokenStore tokenStore;

    public ApiClient(HttpClient httpClient, JwtTokenStore tokenStore)
    {
        this.httpClient = httpClient;
        this.tokenStore = tokenStore;
    }

    public async Task<HttpResponseMessage> GetAsync(string route, CancellationToken cancellationToken = default)
    {
        ApplyToken();
        return await httpClient.GetAsync(route, cancellationToken);
    }

    public async Task<TResponse?> PostAsJsonAsync<TRequest, TResponse>(
        string route,
        TRequest payload,
        CancellationToken cancellationToken = default)
    {
        ApplyToken();
        using var response = await httpClient.PostAsJsonAsync(route, payload, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
    }

    private void ApplyToken()
    {
        httpClient.DefaultRequestHeaders.Authorization = tokenStore.AccessToken is null
            ? null
            : new AuthenticationHeaderValue("Bearer", tokenStore.AccessToken);
    }
}
