namespace Concorde.Abstractions;

public interface IEventManager
{
    public void Register<T, TEvent, THandler>()
        where T : class
        where TEvent : IEvent
        where THandler : IHandler<TEvent>;

    public Task Emit<T, TEvent>(TEvent ev)
        where T : class
        where TEvent : IEvent;
}