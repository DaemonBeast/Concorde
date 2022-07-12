using Concorde.Abstractions;
using Concorde.Abstractions.Client;
using Concorde.Abstractions.Schemas.Events;
using Concorde.Abstractions.Schemas.Rest;
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

    protected override async Task OnReady(ReadyEvent readyEvent)
    {
        this._logger.LogInformation("Logged in as {Tag} with ID {Id}", readyEvent.User.Tag, readyEvent.User.Id);

        var messageCreate = new MessageCreate()
        {
            Content = "Testing this"
        };

        await this.SendMessage(messageCreate, Snowflake.From("992812900353847406"));
    }

    protected override Task OnMessageCreated(MessageCreateEvent messageCreateEvent)
    {
        this._logger.LogInformation("Got message \"{Message}\"", messageCreateEvent.Content);

        return Task.CompletedTask;
    }
}