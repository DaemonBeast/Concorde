using Concorde.Abstractions;
using Concorde.Abstractions.Client;
using Concorde.Abstractions.Schemas.Events;
using Concorde.Client;
using Microsoft.Extensions.Logging;

namespace Concorde.Example;

public class ExampleDiscordClient : BaseDiscordClient
{
    private readonly ILogger<ExampleDiscordClient> _logger;

    public ExampleDiscordClient(
        IDiscordRestClient discordRestClient,
        IDiscordSocketClient discordSocketClient,
        IEventListener<IDiscordSocketClient> eventListener,
        ILogger<ExampleDiscordClient> logger)
        : base(discordRestClient, discordSocketClient, eventListener)
    {
        this._logger = logger;
    }

    protected override Task OnReady(ReadyEvent readyEvent)
    {
        this._logger.LogInformation("Logged in as {Tag} with ID {Id}", readyEvent.User.Tag, readyEvent.User.Id);

        return Task.CompletedTask;
    }
}