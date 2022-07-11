using System.Text.Json.Serialization;
using Concorde.Abstractions.Schemas.Objects;

namespace Concorde.Abstractions.Schemas.Events;

public interface IDiscordEvent : IDiscordObject
{
    [JsonIgnore]
    public string Name { get; }
}