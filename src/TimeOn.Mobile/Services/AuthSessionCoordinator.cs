using TimeOn.Mobile.Interfaces;

namespace TimeOn.Mobile.Services;

public sealed class AuthSessionCoordinator : IAuthSessionCoordinator
{
    public event EventHandler? SessionExpired;

    public void NotifySessionExpired() => SessionExpired?.Invoke(this, EventArgs.Empty);
}
