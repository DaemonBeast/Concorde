using Concorde.Abstractions.Schemas.Events;
using Concorde.Abstractions.Schemas.Socket;

namespace Concorde.Abstractions.Client;

public interface IDiscordSocketClient
{
    public delegate Task ReadyHandler(ReadyEvent readyEvent);
    public event ReadyHandler? OnReady;

    public Task StartAsync(CancellationToken cancellationToken = default);

    public Task StopAsync(CancellationToken cancellationToken = default);
    
    public Task Send<T>(T message) where T : IDiscordSocketMessage;
    
    public Task SendIdentify();
}