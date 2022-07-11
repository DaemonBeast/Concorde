using Concorde.Abstractions.Schemas.Events;

namespace Concorde.Abstractions.Client;

public interface IDiscordSocketClient
{
    public delegate Task ReadyHandler(ReadyEvent readyEvent);
    public event ReadyHandler? OnReady;
    
    public Task StartAsync(CancellationToken cancellationToken = default);

    public Task StopAsync(CancellationToken cancellationToken = default);
    
    public Task SendIdentify();
}