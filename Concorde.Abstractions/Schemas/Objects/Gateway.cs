using System.Text.Json.Serialization;

namespace Concorde.Abstractions.Schemas.Objects;

public class Gateway : IDiscordObject
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}