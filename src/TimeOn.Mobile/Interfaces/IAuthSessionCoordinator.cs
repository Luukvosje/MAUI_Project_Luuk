namespace TimeOn.Mobile.Interfaces;

public interface IAuthSessionCoordinator
{
    event EventHandler? SessionExpired;

    void NotifySessionExpired();
}
