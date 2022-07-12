using System.Text.Json.Serialization;

namespace Concorde.Abstractions.Schemas.Rest;

public class MessageCreate : IDiscordRestObject
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }
    
    // TODO: finish MessageCreate object
}