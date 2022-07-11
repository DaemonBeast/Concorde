using Concorde.Abstractions.Client;
using Concorde.Abstractions.Schemas.Events;
using Concorde.Client;
using Microsoft.Extensions.Logging;

namespace Concorde.Example;

public class ExampleDiscordClient : BaseDiscordClient
{
    private readonly ILogger<ExampleDiscordClient> _logger;

    public ExampleDiscordClient(
        ILogger<ExampleDiscordClient> logger,
        IDiscordRestClient discordRestClient,
        IDiscordSocketClient discordSocketClient)
        : base(discordRestClient, discordSocketClient)
    {
        this._logger = logger;
    }

    protected override Task OnReady(ReadyEvent readyEvent)
    {
        this._logger.LogInformation("Logged in as {Tag} with ID {Id}", readyEvent.User.Tag, readyEvent.User.Id);

        return Task.CompletedTask;
    }
}