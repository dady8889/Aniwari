using Microsoft.AspNetCore.Components;

namespace Aniwari.Shared;

public class DisposableComponentBase : ComponentBase, IDisposable
{
    protected bool Disposed { get; private set; }
    protected CancellationTokenSource CancellationTokenSource { get; init; }

    public DisposableComponentBase() 
    {
        CancellationTokenSource = new();
        Disposed = false;
    }

    public virtual void Dispose()
    {
        Disposed = true;
        CancellationTokenSource.Cancel();
        // CancellationTokenSource.Dispose();
    }
}
