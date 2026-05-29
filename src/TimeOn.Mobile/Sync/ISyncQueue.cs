namespace TimeOn.Mobile.Sync;

public interface ISyncQueue
{
    int Count { get; }
    Task EnqueueAsync(object payload);
    Task<object?> DequeueAsync();
    Task<IReadOnlyList<object>> SnapshotAsync();
}
