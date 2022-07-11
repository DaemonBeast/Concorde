using System.Text.Json.Serialization;

namespace Concorde.Abstractions.Schemas.Socket;

public class IdentifyMessage : DiscordSocketMessage<IdentifyData>
{
    [JsonPropertyName("op")]
    public override int Opcode { get; set; } = (int) Opcodes.Ids.Identity;
    
    public IdentifyMessage() { }
    
    public IdentifyMessage(IdentifyData data) : base(data) { }
}

public class IdentifyData
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("properties")]
    public IdentifyConnectionProperties Properties { get; set; } = new IdentifyConnectionProperties();

    [JsonPropertyName("compress")]
    public bool Compress { get; set; } = false;

    [JsonPropertyName("large_threshold")]
    public int LargeThreshold { get; set; } = 50;

    [JsonPropertyName("shard")]
    public int[] Shard { get; } = new int[] { 0, 1 };
    
    [JsonPropertyName("presence")]
    public PresenceMessage? Presence { get; set; }
    
    [JsonPropertyName("intents")]
    public int Intents { get; set; }

    [JsonIgnore]
    public int ShardId
    {
        get => this.Shard[0];
        set => this.Shard[0] = value;
    }

    [JsonIgnore]
    public int ShardCount
    {
        get => this.Shard[1];
        set => this.Shard[1] = value;
    }
}

public class IdentifyConnectionProperties
{
    [JsonPropertyName("os")]
    public string OperatingSystem { get; set; } = string.Empty;

    [JsonPropertyName("browser")]
    public string Browser { get; set; } = string.Empty;

    [JsonPropertyName("device")]
    public string Device { get; set; } = string.Empty;
}