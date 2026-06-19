using TimeOn.Maui;

namespace TimeOn.UITests.Infrastructure;

/// <summary>
/// Initializes a minimal MAUI application context so XAML pages can inflate and resolve resources.
/// </summary>
public sealed class MauiUiTestHost : IDisposable
{
    private static readonly object Gate = new();
    private static int _referenceCount;

    public MauiUiTestHost()
    {
        lock (Gate)
        {
            if (_referenceCount == 0)
            {
                DispatcherProvider.SetCurrent(new TestDispatcherProvider());
                SQLitePCL.Batteries_V2.Init();
                _ = new App();
            }

            _referenceCount++;
        }
    }

    public void Dispose()
    {
        lock (Gate)
        {
            _referenceCount--;
        }
    }
}
