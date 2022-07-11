namespace Concorde.Abstractions.Schemas.Events;

public class EventDiscordSocketMessage<T> : BaseEventDiscordSocketMessage where T : class, new()
{
    public new T Data { get; set; } = new T();
}

public class BaseEventDiscordSocketMessage
{
    public object Data { get; set; } = new object();
}