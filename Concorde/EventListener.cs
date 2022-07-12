using Concorde.Abstractions;

namespace Concorde;

public class EventListener<T> : IEventListener<T> where T : class
{
    private readonly IEventManager _eventManager;

    public EventListener(IEventManager eventManager)
    {
        this._eventManager = eventManager;
    }

    public void On<TEvent>(Func<TEvent, Task> eventHandler) where TEvent : IEvent
    {
        this._eventManager.Register<T, TEvent>(eventHandler);
    }
}