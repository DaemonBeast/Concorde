using System.Text.Json.Serialization;

namespace Concorde.Abstractions.Schemas.Socket;

public class DispatchDiscordSocketMessage<T> : BaseDispatchDiscordSocketMessage
{
    [JsonPropertyName("d")]
    public T Data { get; set; } = default!;
    
    public DispatchDiscordSocketMessage() { }

    public DispatchDiscordSocketMessage(T data)
    {
        this.Data = data;
    }
}

public class BaseDispatchDiscordSocketMessage : BaseDiscordSocketMessage
{
    public override int Opcode { get; set; } = (int) Opcodes.Ids.Dispatch;

    [JsonPropertyName("s")]
    public int SequenceNumber { get; set; }

    [JsonPropertyName("t")]
    public string EventName { get; set; } = null!;
}