namespace TimeOn.Maui.Interfaces;

public interface ITokenRefreshService
{
    Task<bool> TryRefreshAsync();
}
