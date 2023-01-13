namespace Aniwari.BL.Messaging;

public class Subscription<T> : ISubscription<T>, ISubscription where T : IMessage
{
    public Action<T> ActionHandler { get; private set; }

    public Type ActionType { get; private set; }

    public Subscription(Action<T> action)
    {
        ActionHandler = action;
        ActionType = typeof(T);
    }
}
