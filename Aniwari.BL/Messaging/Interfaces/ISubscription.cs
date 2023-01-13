namespace Aniwari.BL.Messaging;

public interface ISubscription
{
    Type ActionType { get; }
}

public interface ISubscription<T> where T : IMessage
{
    Action<T> ActionHandler { get; }
}
