using System.Text.Json.Serialization;

namespace Concorde.Abstractions.Schemas.Socket;

public class HeartbeatMessage : DiscordSocketMessage<int?>
{
    [JsonPropertyName("op")]
    public override int Opcode { get; set; } = (int) Opcodes.Ids.Heartbeat;
    
    public HeartbeatMessage() { }
    
    public HeartbeatMessage(int? data) : base(data) { }
}