using System.Text.Json.Serialization;
using Concorde.Abstractions.Schemas.Objects;

namespace Concorde.Abstractions.Schemas.Events;

public class MessageCreateEvent : Message, IDiscordEvent
{
    [JsonIgnore]
    public string Name => Events.Names.MessageCreate;
    
    // TODO: finish MessageCreateEvent object
}