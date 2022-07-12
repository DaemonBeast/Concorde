using System.Collections.Concurrent;
using Concorde.Abstractions;
using Concorde.Utilities;

namespace Concorde;

public class EventManager : IEventManager
{
    public void Register<T, TEvent>(Func<TEvent, Task> eventHandler) where T : class where TEvent : IEvent
    {
        EventManager<T, TEvent>.SemaphoreLocker.Lock(() =>
        {
            EventManager<T, TEvent>.EventHandlers.Add(eventHandler);
        });
    }

    public async Task Emit<T, TEvent>(TEvent ev) where T : class where TEvent : IEvent
    {
        await EventManager<T, TEvent>.SemaphoreLocker.LockAsync(async () =>
        {
            foreach (var eventHandler in EventManager<T, TEvent>.EventHandlers)
            {
                await eventHandler.Invoke(ev);
            }
        });
    }
}

internal static class EventManager<T, TEvent> where T : class where TEvent : IEvent
{
    // ReSharper disable once StaticMemberInGenericType
    internal static SemaphoreLocker SemaphoreLocker { get; }
    
    internal static List<Func<TEvent, Task>> EventHandlers { get; }

    static EventManager()
    {
        SemaphoreLocker = new SemaphoreLocker();
        EventHandlers = new List<Func<TEvent, Task>>();
    }
}