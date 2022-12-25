using Microsoft.AspNetCore.Components;
using Aniwari.BL.Services;

namespace Aniwari.Shared;

public class ReactiveComponentBase : DisposableComponentBase
{
#nullable disable
    [Inject]
    protected IMessageBusService MessageBusService { get; private set; }
#nullable restore

    protected List<ISubscription> Subscriptions { get; init; }

    public ReactiveComponentBase()
    {
        Subscriptions = new();
    }

    public void ReactTo<T>(Action<T> handler) where T : IMessage
    {
        var sub = MessageBusService.Subscribe(handler);
        Subscriptions.Add(sub);
    }

    public override void Dispose()
    {
        base.Dispose();

        foreach (var subscription in Subscriptions)
        {
            MessageBusService.Unsubscribe(subscription);
        }

        Subscriptions.Clear();
    }
}
