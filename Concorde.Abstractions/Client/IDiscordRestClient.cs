using Concorde.Abstractions.Schemas.Objects;
using Concorde.Abstractions.Schemas.Rest;

namespace Concorde.Abstractions.Client;

public interface IDiscordRestClient
{
    public Task StartAsync(CancellationToken cancellationToken = default);

    public Task StopAsync(CancellationToken cancellationToken = default);

    public Task<T?> Get<T>(string requestUri);

    public Task<T?> Post<T, TData>(string requestUri, TData data) where TData : IDiscordRestObject;
    
    public Task<Gateway> GetGateway();

    public Task<Message> SendMessage(MessageCreate messageCreate, Snowflake messageId);
}