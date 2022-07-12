namespace Concorde.Abstractions;

public interface IEventManager
{
    public void Register<T, TEvent>(Func<TEvent, Task> eventHandler) where T : class where TEvent : IEvent;

    public Task Emit<T, TEvent>(TEvent ev) where T : class where TEvent : IEvent;
}