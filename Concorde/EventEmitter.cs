using Concorde.Abstractions;

namespace Concorde;

public class EventEmitter<T> : IEventEmitter<T> where T : class
{
    private readonly IEventManager _eventManager;

    public EventEmitter(IEventManager eventManager)
    {
        this._eventManager = eventManager;
    }

    public Task Emit<TEvent>(TEvent ev) where TEvent : IEvent
    {
        return this._eventManager.Emit<T, TEvent>(ev);
    }
}