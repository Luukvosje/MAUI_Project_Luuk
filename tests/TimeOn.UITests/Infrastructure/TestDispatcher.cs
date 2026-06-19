namespace TimeOn.UITests.Infrastructure;

internal sealed class TestDispatcherTimer : IDispatcherTimer
{
    public bool IsRepeating { get; set; }

    public bool IsRunning { get; private set; }

    public TimeSpan Interval { get; set; }

    public event EventHandler? Tick;

    public void Start() => IsRunning = true;

    public void Stop() => IsRunning = false;

    internal void RaiseTick() => Tick?.Invoke(this, EventArgs.Empty);
}

internal sealed class TestDispatcher : IDispatcher
{
    public bool IsDispatchRequired => false;

    public IDispatcherTimer CreateTimer() => new TestDispatcherTimer();

    public bool Dispatch(Action action)
    {
        action();
        return true;
    }

    public bool DispatchDelayed(TimeSpan delay, Action action) => Dispatch(action);

    public Task DispatchAsync(Action action)
    {
        action();
        return Task.CompletedTask;
    }

    public Task DispatchDelayedAsync(TimeSpan delay, Action action) => DispatchAsync(action);
}

internal sealed class TestDispatcherProvider : IDispatcherProvider
{
    private readonly TestDispatcher _dispatcher = new();

    public IDispatcher GetForCurrentThread() => _dispatcher;
}
