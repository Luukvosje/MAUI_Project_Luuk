namespace TimeOn.Maui.Interfaces;

public interface IAuthSessionCoordinator
{
    event EventHandler? SessionExpired;

    void NotifySessionExpired();
}
