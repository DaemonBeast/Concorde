using Concorde.Abstractions;
using Concorde.Abstractions.Client;
using Concorde.Abstractions.Schemas.Events;
using Concorde.Abstractions.Schemas.Objects;
using Concorde.Abstractions.Schemas.Rest;
using Concorde.Extensions;
using Microsoft.Extensions.Hosting;

namespace Concorde.Client;

public class BaseDiscordClient : BackgroundService, IDiscordClient
{
    private readonly IDiscordRestClient _discordRestClient;
    private readonly IDiscordSocketClient _discordSocketClient;
    private readonly IEventListener<IDiscordSocketClient> _socketEventListener;

    public BaseDiscordClient(
        IDiscordRestClient discordRestClient,
        IDiscordSocketClient discordSocketClient,
        IEventListener<IDiscordSocketClient> socketEventListener)
    {
        this._discordRestClient = discordRestClient;
        this._discordSocketClient = discordSocketClient;
        this._socketEventListener = socketEventListener;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await this._discordRestClient.StartAsync(stoppingToken);
        await this._discordSocketClient.StartAsync(stoppingToken);

        await stoppingToken.AsTask();

        await this._discordSocketClient.StopAsync(CancellationToken.None);
        await this._discordRestClient.StopAsync(CancellationToken.None);
    }

    public async Task<Message> SendMessage(MessageCreate messageCreate, Snowflake channelId)
    {
        return await this._discordRestClient.SendMessage(messageCreate, channelId);
    }
}