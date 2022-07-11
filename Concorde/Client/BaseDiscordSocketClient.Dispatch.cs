using Concorde.Abstractions.Schemas.Events;
using Concorde.Abstractions.Schemas.Socket;
using Microsoft.Extensions.Logging;

namespace Concorde.Client;

public partial class BaseDiscordSocketClient
{
    public async Task HandleDispatchAsync(BaseDispatchDiscordSocketMessage message)
    {
        switch (message.EventName)
        {
            case Events.Names.Ready:
            {
                var readyEvent = ((DispatchDiscordSocketMessage<ReadyEvent>) message).Data;

                this._sessionId = readyEvent.SessionId;

                this._logger.LogDebug("Received ready with session ID {SessionId}", readyEvent.SessionId);
                
                this.OnReady?.Invoke(readyEvent);

                break;
            }
            case Events.Names.Resumed:
            {
                this._logger.LogDebug("Resumed");

                break;
            }
        }
    }
}