using System.Text.Json.Serialization;

namespace Concorde.Abstractions.Schemas.Objects;

public class User : IDiscordObject
{
    [JsonPropertyName("id")]
    public string IdString { get; set; } = string.Empty;

    public Snowflake Id => _snowflake ??= Snowflake.From(this.IdString);
    
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;
    
    [JsonPropertyName("discriminator")]
    public string Discriminator { get; set; } = string.Empty;
    
    [JsonIgnore]
    public string Tag => $"{this.Username}#{this.Discriminator}";

    private Snowflake? _snowflake;
}