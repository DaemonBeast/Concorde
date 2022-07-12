namespace Concorde.Abstractions;

public interface IEventListener<out T> where T : class
{
    public void On<TEvent>(Func<TEvent, Task> eventHandler) where TEvent : IEvent;
}