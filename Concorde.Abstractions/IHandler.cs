namespace Concorde.Abstractions;

public interface IHandler<TEvent>
    where TEvent : IEvent
{
    public Task Handle(TEvent ev);
}