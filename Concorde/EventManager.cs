using System.Collections.Concurrent;
using Concorde.Abstractions;

namespace Concorde;

public class EventManager : IEventManager
{
    public void Register<T, TEvent>(Action<TEvent> eventHandler) where T : class where TEvent : IEvent
    {
        EventManager<T, TEvent>.EventHandlers.Add(eventHandler);
    }

    public void Register<T, TEvent>(Func<TEvent, Task> eventHandler) where T : class where TEvent : IEvent
    {
        EventManager<T, TEvent>.AsyncEventHandlers.Add(eventHandler);
    }

    public async Task Emit<T, TEvent>(TEvent ev) where T : class where TEvent : IEvent
    {
        foreach (var eventHandler in EventManager<T, TEvent>.EventHandlers)
        {
            eventHandler.Invoke(ev);
        }

        foreach (var eventHandler in EventManager<T, TEvent>.AsyncEventHandlers)
        {
            await eventHandler.Invoke(ev);
        }
    }
}

internal static class EventManager<T, TEvent> where T : class where TEvent : IEvent
{
    public static ConcurrentBag<Action<TEvent>> EventHandlers { get; }

    public static ConcurrentBag<Func<TEvent, Task>> AsyncEventHandlers { get; }

    static EventManager()
    {
        EventHandlers = new ConcurrentBag<Action<TEvent>>();
        AsyncEventHandlers = new ConcurrentBag<Func<TEvent, Task>>();
    }
}