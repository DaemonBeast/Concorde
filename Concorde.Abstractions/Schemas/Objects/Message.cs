using System.Text.Json.Serialization;

namespace Concorde.Abstractions.Schemas.Objects;

public class Message : IDiscordObject
{
    [JsonPropertyName("id")]
    public string IdString { get; set; } = string.Empty;
    
    public Snowflake Id => _snowflake ??= Snowflake.From(this.IdString);

    [JsonPropertyName("channel_id")]
    public string ChannelIdString { get; set; } = string.Empty;
    
    public Snowflake ChannelId => _channelSnowflake ??= Snowflake.From(this.ChannelIdString);
    
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
    
    private Snowflake? _snowflake;
    private Snowflake? _channelSnowflake;
    
    // TODO: finish Message object
}