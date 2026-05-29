namespace TimeOn.Mobile.Interfaces;

public interface ISyncService
{
    Task SyncPendingChangesAsync();
    bool HasPendingChanges { get; }
}
