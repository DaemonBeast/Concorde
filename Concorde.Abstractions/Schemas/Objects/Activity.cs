using System.Text.Json.Serialization;

namespace Concorde.Abstractions.Schemas.Objects;

public class Activity : IDiscordObject
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("type")]
    public int Type { get; set; }
    
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}