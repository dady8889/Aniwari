using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Aniwari.Shared;

public class DisposableComponentBase : ComponentBase, IDisposable
{
    protected bool Disposed { get; private set; }

    [Parameter]
    public CancellationTokenSource CancellationTokenSource { get; set; } = new();

    public DisposableComponentBase() 
    {
        Disposed = false;
    }

    public virtual void Dispose()
    {
        Disposed = true;
        CancellationTokenSource.Cancel();
        CancellationTokenSource.Dispose();
    }
}
