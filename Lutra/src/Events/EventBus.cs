using System.Collections.Generic;
using Lutra.Utility;

namespace Lutra.Events;

/// <summary>
/// This is the main entry point of the Lutra Event Bus.
/// Dispatch messages (responded to immediately) with EventBus.Dispatch().
/// Queue messages (responded to when the message queue is flushed) with EventBus.Queue().
/// Flush the message queue with EventBus.Flush().
/// Subscribe to messages with EventBus.Subscribe().
/// Unsubscribe to messages when your listener no longer needs them with EventBus.Unsubscribe().
/// You should subclass EventMessage to produce your own message types.
/// </summary>
public static class EventBus
{
    private static Dictionary<Type, Dictionary<object, Action<object>>> Subscribers = new();
    private static List<(Type, dynamic)> QueuedMessages = new();

    public static void Dispatch<T>(T message) where T : EventMessage
    {
        if (!Subscribers.ContainsKey(typeof(T)))
            return;

        foreach (var subscriber in Subscribers[typeof(T)])
        {
            subscriber.Value(message);
        }
    }

    public static void Queue<T>(T message) where T : EventMessage
    {
        QueuedMessages.Add((typeof(T), message));
    }

    public static void Flush()
    {
        foreach (var msg in QueuedMessages)
        {
            Dispatch(Convert.ChangeType(msg.Item2, msg.Item1));
        }

        QueuedMessages.Clear();
    }

    public static void Subscribe<T>(object subscriber, Action<object> callback) where T : EventMessage
    {
        if (!Subscribers.ContainsKey(typeof(T)))
        {
            Subscribers.Add(typeof(T), new Dictionary<object, Action<object>>());
        }
        
        Subscribers[typeof(T)][subscriber] = callback;
    }

    public static void Unsubscribe<T>(object subscriber) where T : EventMessage
    {
        if (!Subscribers.ContainsKey(typeof(T)))
            return;

        if (Subscribers[typeof(T)].ContainsKey(subscriber))
        {
            Subscribers[typeof(T)].Remove(subscriber);
        }
    }
}