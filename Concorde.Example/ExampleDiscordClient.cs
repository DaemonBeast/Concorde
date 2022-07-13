using Concorde.Abstractions;
using Concorde.Abstractions.Client;
using Concorde.Abstractions.Schemas.Events;
using Concorde.Abstractions.Schemas.Rest;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Concorde.Example;

public class ExampleDiscordClient : BackgroundService
{
    private readonly IEventListener<IDiscordSocketClient> _eventListener;

    public ExampleDiscordClient(IEventListener<IDiscordSocketClient> eventListener)
    {
        this._eventListener = eventListener;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        this._eventListener.On<ReadyEvent, ReadyHandler>();
        this._eventListener.On<MessageCreateEvent, MessageCreatedHandler>();

        return Task.CompletedTask;
    }

    public class ReadyHandler : IHandler<ReadyEvent>
    {
        private readonly IDiscordClient _discordClient;
        private readonly ILogger<ReadyHandler> _logger;

        public ReadyHandler(IDiscordClient discordClient, ILogger<ReadyHandler> logger)
        {
            this._discordClient = discordClient;
            this._logger = logger;
        }

        public async Task Handle(ReadyEvent readyEvent)
        {
            this._logger.LogInformation("Logged in as {Tag} with ID {Id}", readyEvent.User.Tag, readyEvent.User.Id);

            var messageCreate = new MessageCreate()
            {
                Content = "Testing this"
            };

            await this._discordClient.SendMessage(messageCreate, Snowflake.From("992812900353847406"));
        }
    }
    
    public class MessageCreatedHandler : IHandler<MessageCreateEvent>
    {
        private readonly IDiscordClient _discordClient;
        private readonly ILogger<MessageCreatedHandler> _logger;

        public MessageCreatedHandler(IDiscordClient discordClient, ILogger<MessageCreatedHandler> logger)
        {
            this._discordClient = discordClient;
            this._logger = logger;
        }

        public Task Handle(MessageCreateEvent messageCreateEvent)
        {
            this._logger.LogInformation("Got message \"{Message}\"", messageCreateEvent.Content);

            return Task.CompletedTask;
        }
    }
}