/*using System.Buffers;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Concorde.Extensions;
using Concorde.Schemas.Rest;
using Concorde.Schemas.WebSocket;
using Concorde.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.HighPerformance;
using Microsoft.Toolkit.HighPerformance.Buffers;
using Timer = System.Timers.Timer;

namespace Concorde;

public class ClientBase : IClient
{
    public delegate void ReadyHandler();
    public event ReadyHandler? OnReady;

    protected readonly ILogger<ClientBase> Logger;
    protected readonly HttpClient HttpClient;

    protected int? LastSequenceNumber = null;
    
    private readonly ClientWebSocket _webSocket;

    private readonly CancellationTokenSource _webSocketTokenSource;
    private readonly CancellationToken _webSocketToken;

    private readonly Timer _heartbeatTimer;

    public ClientBase(ILogger<ClientBase> logger, HttpClient httpClient, ClientWebSocket webSocket)
    {
        this.Logger = logger;
        this.HttpClient = httpClient;
        this._webSocket = webSocket;

        this._webSocketTokenSource = new CancellationTokenSource();
        this._webSocketToken = this._webSocketTokenSource.Token;

        this._heartbeatTimer = new Timer();
        this._heartbeatTimer.AutoReset = true;
        this._heartbeatTimer.Elapsed += async (_, _) => await this.Heartbeat();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        this.ConfigureHttpClient();

        await this.ConnectWebSocket(cancellationToken);
        
        this.StartReceiveAsync();

        /*_ = Task.Run(async () =>
        {
            var memory = new Memory<byte>(new byte[1000]);
            await this._webSocket.ReceiveAsync(memory, cancellationToken);

            // await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            memory = memory.TrimEnd<byte>(0);
            
            

            Type type;

            // var str = Encoding.UTF8.GetString(memory.Span);
            using (var stream = new MemoryStream(memory.ToArray()))
            {
                var json = await JsonSerializer.DeserializeAsync<BaseMessage>(stream, cancellationToken: cancellationToken);
                type = Opcodes.OpcodeSchemas[json!.Opcode];

                this._logger.LogInformation("Opcode: {Opcode} ({Id})", (Opcodes.OpcodeIds)json.Opcode, json.Opcode);
            }

            using (var stream = new MemoryStream(memory.ToArray()))
            {
                var json = (await JsonSerializer.DeserializeAsync(stream, type, cancellationToken: cancellationToken))!;
                this._logger.LogInformation("{Buffer}", Encoding.UTF8.GetString(memory.Span));

                switch (json)
                {
                    case HelloMessage hello:
                    {
                        this._logger.LogInformation(
                            "Heartbeat interval: {HeartbeatInterval}",
                            hello.Data.HeartbeatInterval);
                    
                        break;
                    }
                }
            }
        }, cancellationToken);*/
    /*}

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        this._webSocketTokenSource.Cancel();
        this._webSocketTokenSource.Dispose();
        
        this.HttpClient.Dispose();

        await this._webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Bot client stopped", cancellationToken);

        this._webSocket.Dispose();
    }

    public void StartReceiveAsync()
    {
        _ = Task.Run(this.ReceiveMessagesAsync, this._webSocketToken);
    }

    public async Task ReceiveMessagesAsync()
    {
        while (!this._webSocketToken.IsCancellationRequested)
        {
            var memoryOwner = MemoryOwner<byte>.Allocate(4096, AllocationMode.Clear);
            var memory = memoryOwner.Memory;

            // TODO: account for fragmentation
            var result = await this._webSocket.ReceiveAsync(memory, this._webSocketToken);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                this.Logger.LogError(
                    "Web socket was closed by remote: {Reason}", this._webSocket.CloseStatusDescription);

                throw new Exception($"Web socket was closed by remote: {this._webSocket.CloseStatusDescription}");
            }

            memoryOwner = memoryOwner[..result.Count];

            _ = Task.Run(async () => await this.ProcessMessageAsync(memoryOwner), this._webSocketToken);
        }
    }

    public async Task ProcessMessageAsync(MemoryOwner<byte> memoryOwner)
    {
        await using var stream = memoryOwner.AsStream();
        
        this.Logger.LogInformation("{0}", Encoding.UTF8.GetString(memoryOwner.Span));
        
        var baseMessage =
            await JsonSerializer.DeserializeAsync<BaseMessage>(stream, cancellationToken: this._webSocketToken);

        if (baseMessage == null)
        {
            this.Logger.LogWarning("Received invalid web socket message");
            return;
        }

        var opcodeId = baseMessage.Opcode;
        if (!Opcodes.OpcodeSchemas.TryGetValue(opcodeId, out var messageType))
        {
            var opcodeExists = Enum.IsDefined(typeof(Opcodes.OpcodeIds), opcodeId);

            if (opcodeExists)
            {
                this.Logger.LogWarning(
                    "Received non-implemented opcode {Opcode} ({Id})", (Opcodes.OpcodeIds) opcodeId, opcodeId);
            }
            else
            {
                this.Logger.LogWarning("Received non-implemented opcode {Id}", opcodeId);
            }

            return;
        }

        this.Logger.LogInformation(
            "Received opcode {Opcode} ({Id})", (Opcodes.OpcodeIds) opcodeId, opcodeId);

        BaseMessage? message;

        if (messageType == typeof(BaseMessage))
        {
            // no need to deserialize again
            message = baseMessage;
        }
        else
        {
            stream.Position = 0;

            message = await JsonSerializer.DeserializeAsync(
                stream,
                messageType,
                cancellationToken: this._webSocketToken) as BaseMessage;
        }

        if (message == null)
        {
            // do stuff
            return;
        }

        switch ((Opcodes.OpcodeIds) message.Opcode)
        {
            case Opcodes.OpcodeIds.Hello:
            {
                this.Logger.LogInformation(
                    "Got heartbeat interval {HeartbeatInterval}s", helloMessage.Data.HeartbeatInterval / 1000f);
                
                await this.SendHeartbeat();

                await this.SendIdentify();
                
                this.StartHeartbeat(helloMessage.Data.HeartbeatInterval);

                /*_ = Task.Run(async () =>
                {
                    // var delay = (int) (helloMessage.Data.HeartbeatInterval * new Random().NextSingle());
                    // await Task.Delay(delay, this._webSocketToken);
                        
                    // await this.SendHeartbeat();

                    this.StartHeartbeat(helloMessage.Data.HeartbeatInterval);
                }, this._webSocketToken);

                /*this.Logger.LogInformation("invoking");
                this.OnReady?.Invoke();
                this.Logger.LogInformation("invoked");*/
                    
                /*break;
            }
        }
    }

    public async Task<Gateway> GetGateway()
    {
        // TODO: cancellation support
        return await this.HttpClient.GetFromJsonAsync<Gateway>("gateway")
               ?? throw new Exception("Failed to get gateway URL");
    }
    
    private void StartHeartbeat(int heartbeatInterval)
    {
        this._heartbeatTimer.Interval = heartbeatInterval;
        this._heartbeatTimer.Enabled = true;
    }

    private async Task Heartbeat()
    {
        // TODO: handle heartbeat acks

        await this.SendHeartbeat();
    }

    private async Task SendHeartbeat()
    {
        var heartbeat = new HeartbeatMessage()
        {
            Data = this.LastSequenceNumber
        };
        
        // var data = await heartbeat.SerializeAsync(cancellationToken: this._webSocketToken);
        
        var memoryOwner = MemoryOwner<byte>.Allocate(4096, AllocationMode.Clear);
        await using (var stream = memoryOwner.AsStream())
        {
            await JsonSerializer.SerializeAsync(stream, heartbeat, cancellationToken: this._webSocketToken);

            var data = memoryOwner.Memory[..(int) stream.Position];

            await this._webSocket.SendAsync(data, WebSocketMessageType.Binary, true, this._webSocketToken);
        }
    }

    private async Task SendIdentify()
    {
        var identify = new IdentifyMessage()
        {
            Data = new IdentifyData()
            {
                Token = "OTc5NzM4NjgwOTEyNjUwMjYw.GLNOFR.PkWJJV4-JFNwGs8wv9QxoiTBlSj9MjDPeKocxA",
                Intents = 0b111111111111111111111,
                Properties = new IdentifyConnectionProperties()
                {
                    OperatingSystem = RuntimeInformation.RuntimeIdentifier,
                    Browser = "Concorde",
                    Device = "Concorde"
                }
            }
        };
        
        // var data = await identify.SerializeAsync(cancellationToken: this._webSocketToken);
        
        var memoryOwner = MemoryOwner<byte>.Allocate(4096, AllocationMode.Clear);
        await using (var stream = memoryOwner.AsStream())
        {
            await JsonSerializer.SerializeAsync(stream, identify, cancellationToken: this._webSocketToken);

            var data = memoryOwner.Memory[..(int) stream.Position];

            await this._webSocket.SendAsync(data, WebSocketMessageType.Binary, true, this._webSocketToken);
        }
    }

    private void ConfigureHttpClient()
    {
        this.HttpClient.BaseAddress = DiscordApiUtilities.GetApiUrl();

        var headers = this.HttpClient.DefaultRequestHeaders;
        headers.Authorization = new AuthenticationHeaderValue("Bot", "OTc5NzM4NjgwOTEyNjUwMjYw.GLNOFR.PkWJJV4-JFNwGs8wv9QxoiTBlSj9MjDPeKocxA");
        headers.UserAgent.ParseAdd(DiscordApiUtilities.GetUserAgent());
    }

    private async Task ConnectWebSocket(CancellationToken cancellationToken)
    {
        var gatewayUrl = (await this.GetGateway()).Url;
        var gatewayUri = DiscordApiUtilities.GetGatewayUrl(gatewayUrl);
        
        this.Logger.LogInformation("Got gateway URL {GatewayUrl}", gatewayUrl);

        await this._webSocket.ConnectAsync(gatewayUri, cancellationToken);
    }
}*/