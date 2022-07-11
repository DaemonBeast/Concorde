using System.Net.WebSockets;
using System.Text.Json;
using Concorde.Abstractions;
using Concorde.Abstractions.Client;
using Concorde.Json;
using Concorde.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace Concorde.Client;

public partial class BaseDiscordSocketClient : IDiscordSocketClient
{
    protected ClientWebSocket? Socket;
    
    private readonly ILogger<BaseDiscordSocketClient> _logger;
    private readonly IConfiguration _configuration;
    private readonly IFactory<ClientWebSocket> _socketFactory;
    private readonly IDiscordRestClient _discordRestClient;

    private CancellationTokenSource _socketTokenSource;
    private CancellationToken _socketToken;

    private CancellationTokenSource _socketReceiveTokenSource;
    private CancellationToken _socketReceiveToken;

    private Timer? _heartbeatTimer;
    private int _heartbeatInterval;
    private bool _heartbeatAcked;

    private bool _resuming;

    private readonly string _botToken;

    public BaseDiscordSocketClient(
        ILogger<BaseDiscordSocketClient> logger,
        IConfiguration configuration,
        IFactory<ClientWebSocket> socketFactory,
        IDiscordRestClient discordRestClient)
    {
        this._logger = logger;
        this._configuration = configuration;
        this._socketFactory = socketFactory;
        this._discordRestClient = discordRestClient;

        this._socketTokenSource = new CancellationTokenSource();
        this._socketToken = this._socketTokenSource.Token;

        this._socketReceiveTokenSource = new CancellationTokenSource();
        this._socketReceiveToken = this._socketReceiveTokenSource.Token;

        this._socketSerializerOptions = new JsonSerializerOptions()
        {
            Converters =
            {
                new DiscordSocketMessageConverter()
            }
        };

        this._botToken = this._configuration.GetValue<string>("token") ??
                         throw new Exception(
                             "Bot token must be specified via the command line or environment variables");
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await this.SetupSocket(cancellationToken);
        
        this.StartHandleMessages();
        
        this._logger.LogTrace("Started Discord socket client");
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await this.CloseSocket(cancellationToken);
        
        this._heartbeatTimer?.Close();
        
        this._logger.LogTrace("Stopped Discord socket client");
    }

    private async Task Heartbeat()
    {
        this._logger.LogTrace("Heartbeat");
        
        if (!this._heartbeatAcked && this.Socket != null)
        {
            try
            {
                await this.Socket.CloseAsync(
                    WebSocketCloseStatus.ProtocolError,
                    "Heartbeat was not acknowledged",
                    this._socketToken);
            }
            catch
            {
                // ignored
            }
        }
        
        if (this.Socket?.State != WebSocketState.Open)
        {
            this.ResetHeartbeat();

            await this.ResetSocket(true);
            await this.SendResume();
            
            return;
        }

        this._heartbeatAcked = false;

        await this.SendHeartbeat();
    }

    private async Task SetupSocket(CancellationToken cancellationToken = default)
    {
        this._logger.LogTrace("Setting up socket");
        
        var gatewayUrl = (await this._discordRestClient.GetGateway()).Url;
        var gatewayUri = DiscordApiUtilities.GetGatewayUrl(gatewayUrl);

        this.Socket = this._socketFactory.Create();
        
        this._logger.LogTrace("Connecting to gateway");

        await this.Socket.ConnectAsync(gatewayUri, cancellationToken);
        
        this._socketReceiveTokenSource = new CancellationTokenSource();
        this._socketReceiveToken = this._socketReceiveTokenSource.Token;

        this._logger.LogTrace("Set up socket");
    }

    private async Task ResetSocket(bool resume = false)
    {
        if (this._socketToken.IsCancellationRequested || this._socketReceiveToken.IsCancellationRequested)
        {
            return;
        }
        
        this._logger.LogDebug("Resetting socket");

        this._resuming = resume;
        
        await this.DisposeSocket(this._socketToken);
        await this.SetupSocket(this._socketToken);
    }

    private async Task CloseSocket(CancellationToken cancellationToken = default)
    {
        this._logger.LogTrace("Closing socket");
        
        if (!this._socketReceiveTokenSource.IsCancellationRequested)
        {
            this._socketReceiveTokenSource.Cancel();
            this._socketReceiveTokenSource.Dispose();
        }

        if (this.Socket != null)
        {
            try
            {
                await this.Socket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Bot client stopped",
                    cancellationToken);
            }
            catch
            {
                // ignored
            }
        }
    }

    private async Task DisposeSocket(CancellationToken cancellationToken = default)
    {
        this._logger.LogTrace("Disposing socket");
        
        await this.CloseSocket(cancellationToken);
        
        this.Socket?.Dispose();
    }

    private void StartHeartbeat(int heartbeatInterval)
    {
        this._heartbeatInterval = heartbeatInterval;
        
        if (this._heartbeatTimer == null)
        {
            this.ResetHeartbeat();
        }
        
        this._heartbeatTimer!.Interval = heartbeatInterval;
        this._heartbeatTimer.Start();
    }

    private void SetupHeartbeat()
    {
        this._heartbeatTimer = new Timer();
        this._heartbeatTimer.AutoReset = true;
        this._heartbeatTimer.Elapsed += async (_, _) => await this.Heartbeat();
    }

    private void ResetHeartbeat()
    {
        this._heartbeatTimer?.Close();
        
        this.SetupHeartbeat();
    }
    
    private void RestartHeartbeat(int? heartbeatInterval = null)
    {
        this.ResetHeartbeat();
        
        this.StartHeartbeat(heartbeatInterval ?? this._heartbeatInterval);
    }
}

// TODO: add http and socket rate limits