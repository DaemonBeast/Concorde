namespace Concorde.Abstractions;

public interface IEventEmitter<out T> where T : class
{
    public void Emit<TEvent>(TEvent ev) where TEvent : IEvent;
}