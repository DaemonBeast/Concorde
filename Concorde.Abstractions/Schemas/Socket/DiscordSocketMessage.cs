using System.Text.Json.Serialization;

namespace Concorde.Abstractions.Schemas.Socket;

public class DiscordSocketMessage<T> : BaseDiscordSocketMessage
{
    [JsonPropertyName("d")]
    public T Data { get; set; } = default!;
    
    public DiscordSocketMessage() { }

    public DiscordSocketMessage(T data)
    {
        this.Data = data;
    }
}

public class BaseDiscordSocketMessage : IDiscordSocketMessage
{
    [JsonPropertyName("op")]
    public virtual int Opcode { get; set; }
}