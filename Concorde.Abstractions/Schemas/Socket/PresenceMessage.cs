using System.Text.Json.Serialization;
using Concorde.Abstractions.Schemas.Objects;

namespace Concorde.Abstractions.Schemas.Socket;

public class PresenceMessage : DiscordSocketMessage<PresenceData>
{
    [JsonPropertyName("op")]
    public override int Opcode { get; set; } = (int) Opcodes.Ids.PresenceUpdate;
    
    public PresenceMessage() { }
    
    public PresenceMessage(PresenceData data) : base(data) { }
}

public class PresenceData
{
    [JsonPropertyName("since")]
    public int? IdleSince { get; set; }
    
    [JsonPropertyName("activities")]
    public Activity[] Activities { get; set; } = Array.Empty<Activity>();
    
    [JsonPropertyName("status")]
    public string Status { get; set; } = StatusTypes.Online;
        
    [JsonPropertyName("afk")]
    public bool Afk { get; set; }
}

public static class StatusTypes
{
    public const string Online = "online";
    public const string DoNotDisturb = "dnd";
    public const string Idle = "idle";
    public const string Invisible = "invisible";
    public const string Offline = "offline";
}