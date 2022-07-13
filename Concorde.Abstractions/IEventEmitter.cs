namespace Concorde.Abstractions;

public interface IEventEmitter<out T> where T : class
{
    public Task Emit<TEvent>(TEvent ev)
        where TEvent : IEvent;
}