using System.Text.Json.Serialization;
using Concorde.Abstractions.Schemas.Objects;

namespace Concorde.Abstractions.Schemas.Socket;

/*public class BaseDataDiscordSocketMessage<T> : BaseDataDiscordSocketMessage where T : new()
{
    [JsonPropertyName("d")]
    public new T Data { get; set; } = new T();
}

public class BaseDataDiscordSocketMessage : BaseDiscordSocketMessage
{
    [JsonPropertyName("d")]
    public object Data { get; set; } = new object();
}

public class BaseDiscordSocketMessage : IDiscordSocketMessage
{
    [JsonPropertyName("op")]
    public virtual int Opcode { get; set; }
}*/