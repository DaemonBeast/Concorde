using Concorde.Abstractions.Schemas.Objects;

namespace Concorde.Abstractions.Client;

public interface IDiscordRestClient
{
    public Task StartAsync(CancellationToken cancellationToken = default);

    public Task StopAsync(CancellationToken cancellationToken = default);
    
    public Task<Gateway> GetGateway();
}