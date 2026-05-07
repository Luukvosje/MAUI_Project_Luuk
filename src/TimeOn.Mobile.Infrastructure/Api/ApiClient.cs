using System.Net.Http.Headers;

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

    private void ApplyToken()
    {
        httpClient.DefaultRequestHeaders.Authorization = tokenStore.AccessToken is null
            ? null
            : new AuthenticationHeaderValue("Bearer", tokenStore.AccessToken);
    }
}
