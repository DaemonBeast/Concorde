using System.Text.Json.Serialization;

namespace Concorde.Abstractions.Schemas.Socket;

public class ResumeMessage : DiscordSocketMessage<ResumeData>
{
    [JsonPropertyName("op")]
    public override int Opcode { get; set; } = (int) Opcodes.Ids.Resume;
    
    public ResumeMessage() { }
    
    public ResumeMessage(ResumeData data) : base(data) { }
}

public class ResumeData
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("session_id")]
    public string SessionId { get; set; } = string.Empty;
    
    [JsonPropertyName("seq")]
    public int LastSequenceNumber { get; set; }
}