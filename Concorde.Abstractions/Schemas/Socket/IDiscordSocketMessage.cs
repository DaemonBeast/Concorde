using System.Text.Json.Serialization;

namespace Concorde.Abstractions.Schemas.Socket;

public interface IDiscordSocketMessage
{
    [JsonPropertyName("op")]
    public int Opcode { get; set; }
}