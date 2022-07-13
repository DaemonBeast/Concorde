namespace Concorde.Abstractions;

public interface IEventListener<out T> where T : class
{
    public void On<TEvent, THandler>()
        where TEvent : IEvent
        where THandler : IHandler<TEvent>;
}