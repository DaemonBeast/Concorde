using System.Text.Json.Serialization;

namespace Concorde.Abstractions.Schemas.Socket;

public class InvalidSessionMessage : DiscordSocketMessage<bool>
{
    [JsonPropertyName("op")]
    public override int Opcode { get; set; } = (int) Opcodes.Ids.InvalidSession;
    
    public InvalidSessionMessage() { }
    
    public InvalidSessionMessage(bool data) : base(data) { }
}