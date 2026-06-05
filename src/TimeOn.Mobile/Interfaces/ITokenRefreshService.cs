namespace TimeOn.Mobile.Interfaces;

public interface ITokenRefreshService
{
    Task<bool> TryRefreshAsync(CancellationToken cancellationToken = default);
}
