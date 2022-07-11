using System.Text.Json.Serialization;
using Concorde.Abstractions.Schemas.Objects;

namespace Concorde.Abstractions.Schemas.Events;

public class ReadyEvent : IDiscordEvent
{
    [JsonIgnore]
    public string Name => Events.Names.Ready;
    
    [JsonPropertyName("v")]
    public int ApiVersion { get; set; }

    [JsonPropertyName("user")]
    public User User { get; set; } = new User();
    
    // TODO: guilds

    [JsonPropertyName("session_id")]
    public string SessionId { get; set; } = string.Empty;
    
    [JsonPropertyName("shard")]
    public int[]? Shard { get; } = new int[] { 0, 1 };
    
    [JsonIgnore]
    public int? ShardId
    {
        get => this.Shard?.GetValue(0) as int?;
        set => this.Shard?.SetValue(value, 0);
    }

    [JsonIgnore]
    public int? ShardCount
    {
        get => this.Shard?.GetValue(0) as int?;
        set => this.Shard?.SetValue(value, 1);
    }
    
    // TODO: application
}