using Concorde.Abstractions;

namespace Concorde;

public class EventListener<T> : IEventListener<T>
    where T : class
{
    private readonly IEventManager _eventManager;

    public EventListener(IEventManager eventManager)
    {
        this._eventManager = eventManager;
    }

    public void On<TEvent, THandler>()
        where TEvent : IEvent
        where THandler : IHandler<TEvent>
    {
        this._eventManager.Register<T, TEvent, THandler>();
    }
}