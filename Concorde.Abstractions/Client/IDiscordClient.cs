using Concorde.Abstractions.Schemas.Objects;
using Concorde.Abstractions.Schemas.Rest;

namespace Concorde.Abstractions.Client;

public interface IDiscordClient
{
    public Task<Message> SendMessage(MessageCreate messageCreate, Snowflake channelId);
}