using Aniwari.BL.Messaging;

namespace Aniwari.BL.Interfaces;

public interface IMessageBusService
{
    void Publish<T>(T message) where T : IMessage;

    ISubscription Subscribe<T>(Action<T> actionCallback) where T : IMessage;

    void Unsubscribe(ISubscription subscription);
}