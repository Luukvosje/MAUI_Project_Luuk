using TimeOn.Maui.Interfaces;

namespace TimeOn.Maui.Services;

public sealed class AuthSessionCoordinator : IAuthSessionCoordinator
{
    public event EventHandler? SessionExpired;

    public void NotifySessionExpired() => SessionExpired?.Invoke(this, EventArgs.Empty);
}
