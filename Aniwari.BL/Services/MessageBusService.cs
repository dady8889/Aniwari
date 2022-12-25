using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aniwari.BL.Services;

public interface IMessage
{
}

public interface ISubscription
{
    Type ActionType { get; }
}

public interface ISubscription<T> where T : IMessage
{
    Action<T> ActionHandler { get; }
}

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

public interface IMessageBusService
{
    void Publish<T>(T message) where T : IMessage;

    ISubscription Subscribe<T>(Action<T> actionCallback) where T : IMessage;

    void Unsubscribe(ISubscription subscription);
}

public class MessageBusService : IMessageBusService
{
    private readonly ILogger<MessageBusService> _logger;

    private readonly Dictionary<Type, List<ISubscription>> _observers = new();

    public MessageBusService(ILogger<MessageBusService> logger)
    {
        _logger = logger;
    }

    public ISubscription Subscribe<T>(Action<T> callback) where T : IMessage
    {
        Type messageType = typeof(T);

        if (!_observers.TryGetValue(messageType, out List<ISubscription>? subscriptions))
        {
            subscriptions = new();
            _observers[messageType] = subscriptions;
        }

        if (!subscriptions
            .Select(s => s as ISubscription<T>)
            .Any(s => s != null && s.ActionHandler == callback))
        {
            var subscription = new Subscription<T>(callback);
            subscriptions.Add(subscription);
            return subscription;
        }
        else 
        {
            _logger.LogError("This event handler is already subscribed to {}", typeof(T).Name);
            throw new Exception($"This event handler is already subscribed to {typeof(T).Name}");
        }
    }

    public void Publish<T>(T message) where T : IMessage
    {
        if (message == null)
        {
            _logger.LogError("Cannot publish an empty message.");
            throw new ArgumentNullException(nameof(message));
        }

        Type messageType = typeof(T);

        if (!_observers.TryGetValue(messageType, out var subscriptions))
            return; // not registered

        if (subscriptions.Count == 0)
            return; // registered, but no observers

        // notify observers
        foreach (var handler in subscriptions
            .Select(s => s as ISubscription<T>)
            .Select(s => s?.ActionHandler))
        {
            handler?.Invoke(message);
        }
    }

    public void Unsubscribe(ISubscription subscription)
    {
        if (subscription == null)
        {
            _logger.LogError("Cannot unsubscribe null.");
            throw new ArgumentNullException(nameof(subscription));
        }

        Type messageType = subscription.ActionType;

        if (_observers.TryGetValue(messageType, out var subscriptions))
        {
            // remove observer
            subscriptions.Remove(subscription);

            // unregister if there are no observers left
            if (subscriptions.Count == 0)
                _observers.Remove(messageType);
        }
        else
        {
            _logger.LogError("This event handler is not subscribed to {}", messageType.Name);
            throw new Exception($"This event handler is not subscribed to {messageType.Name}");
        }
    }
}