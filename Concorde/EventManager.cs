using Concorde.Abstractions;
using Concorde.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Concorde;

public class EventManager : IEventManager
{
    private readonly IServiceProvider _serviceProvider;

    public EventManager(IServiceProvider serviceProvider)
    {
        this._serviceProvider = serviceProvider;
    }
    
    public void Register<T, TEvent, THandler>()
        where T : class
        where TEvent : IEvent
        where THandler : IHandler<TEvent>
    {
        EventManager<T, TEvent>.SemaphoreLocker.Lock(() =>
        {
            EventManager<T, TEvent>.EventHandlers.Add(typeof(THandler));
        });
    }

    public async Task Emit<T, TEvent>(TEvent ev) where T : class where TEvent : IEvent
    {
        await EventManager<T, TEvent>.SemaphoreLocker.LockAsync(async () =>
        {
            foreach (var eventHandlerType in EventManager<T, TEvent>.EventHandlers)
            {
                var eventHandler =
                    (IHandler<TEvent>) ActivatorUtilities.CreateInstance(this._serviceProvider, eventHandlerType);
                
                await eventHandler.Handle(ev);
            }
        });
    }
}

internal static class EventManager<T, TEvent> where T : class where TEvent : IEvent
{
    // ReSharper disable once StaticMemberInGenericType
    internal static SemaphoreLocker SemaphoreLocker { get; }
    
    internal static List<Type> EventHandlers { get; }

    static EventManager()
    {
        SemaphoreLocker = new SemaphoreLocker();
        EventHandlers = new List<Type>();
    }
}