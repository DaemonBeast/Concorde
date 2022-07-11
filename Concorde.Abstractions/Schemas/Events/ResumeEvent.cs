using System.Text.Json.Serialization;

namespace Concorde.Abstractions.Schemas.Events;

public class ResumeEvent : IDiscordEvent
{
    [JsonIgnore]
    public string Name => Events.Names.Resumed;
}