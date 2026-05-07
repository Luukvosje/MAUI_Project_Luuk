namespace TimeOn.Mobile.Infrastructure.Api;

public sealed class JwtTokenStore
{
    public string? AccessToken { get; private set; }

    public void Set(string? token) => AccessToken = token;
}
