using System.Net;
using System.Net.Http.Headers;
using TimeOn.Mobile.Interfaces;

namespace TimeOn.Mobile.Http;

public sealed class BearerTokenRefreshingHandler : DelegatingHandler
{
    private static readonly HttpRequestOptionsKey<bool> RetryAfterRefreshKey = new("RetryAfterRefresh");

    private readonly IAuthTokenStore _tokenStore;
    private readonly ITokenRefreshService _tokenRefreshService;

    public BearerTokenRefreshingHandler(IAuthTokenStore tokenStore, ITokenRefreshService tokenRefreshService)
    {
        _tokenStore = tokenStore;
        _tokenRefreshService = tokenRefreshService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        await ApplyBearerAsync(request);

        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode != HttpStatusCode.Unauthorized
            || request.Options.TryGetValue(RetryAfterRefreshKey, out _))
        {
            return response;
        }

        response.Dispose();

        if (!await _tokenRefreshService.TryRefreshAsync(cancellationToken))
        {
            return new HttpResponseMessage(HttpStatusCode.Unauthorized);
        }

        await ApplyBearerAsync(request);
        request.Options.Set(RetryAfterRefreshKey, true);
        return await base.SendAsync(request, cancellationToken);
    }

    private async Task ApplyBearerAsync(HttpRequestMessage request)
    {
        var token = await _tokenStore.GetAccessTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = null;
            return;
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}
