using System.Text.Json.Serialization;

namespace Concorde.Abstractions.Schemas.Socket;

public class HelloMessage : DiscordSocketMessage<HelloData>
{
    [JsonPropertyName("op")]
    public override int Opcode { get; set; } = (int) Opcodes.Ids.Hello;
    
    public HelloMessage() { }
    
    public HelloMessage(HelloData data) : base(data) { }
}

public class HelloData
{
    [JsonPropertyName("heartbeat_interval")]
    public int HeartbeatInterval { get; set; }
}