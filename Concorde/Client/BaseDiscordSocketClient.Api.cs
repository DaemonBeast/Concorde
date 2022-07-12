using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text.Json;
using Concorde.Abstractions.Client;
using Concorde.Abstractions.Schemas.Rest;
using Concorde.Abstractions.Schemas.Socket;
using Concorde.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.HighPerformance;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace Concorde.Client;

public partial class BaseDiscordSocketClient
{
    public event IDiscordSocketClient.ReadyHandler? OnReady;
    
    private int? _lastSequenceNumber;
    private string? _sessionId;

    private readonly JsonSerializerOptions _socketSerializerOptions;

    private const int MinReceiveBufferSize = 4 * 1024;          // 4 KiB
    private const int MediumReceiveBufferSize = 1024 * 1024;    // 1 MiB
    private const int MaxReceiveBufferSize = 8 * 1024 * 1024;   // 8 MiB

    private const int RetryInterval = 5;
    
    public async Task Send<T>(T message) where T : IDiscordSocketMessage
    {
        this._logger.LogTrace("Sending socket message {MessageType}", typeof(T));
        
        if (this._socketToken.IsCancellationRequested)
        {
            return;
        }

        var memoryOwner = MemoryOwner<byte>.Allocate(4096);
        await using var stream = memoryOwner.AsStream();
        
        await JsonSerializer.SerializeAsync(stream, message, cancellationToken: this._socketToken);

        var data = memoryOwner.Memory[..(int) stream.Position];

        while (true)
        {
            try
            {
                await this.Socket!.SendAsync(data, WebSocketMessageType.Binary, true, this._socketToken);

                break;
            }
            catch
            {
                if (this._socketToken.IsCancellationRequested)
                {
                    break;
                }
                
                this._logger.LogTrace(
                    "Socket message failed to send. Retrying after {RetryInterval} seconds",
                    RetryInterval);
                
                if (message.Opcode is
                    (int) Opcodes.Ids.Heartbeat or
                    (int) Opcodes.Ids.Identity or
                    (int) Opcodes.Ids.Resume)
                {
                    await this.ResetSocket(true);

                    if (this.Socket?.State != WebSocketState.Open)
                    {
                        await this._socketToken.AsTask(TimeSpan.FromSeconds(RetryInterval));
                    }
                }
                else
                {
                    await this.ResetSocketAndWait();
                }
            }
        }

        // await stream.DisposeAsync();
    }

    public async Task SendIdentify()
    {
        var identify = new IdentifyMessage()
        {
            Data = new IdentifyData()
            {
                Token = this._botToken,
                // TODO: make intents not hardcoded
                Intents = 0b111111111111111111111,
                Properties = new IdentifyConnectionProperties()
                {
                    OperatingSystem = RuntimeInformation.RuntimeIdentifier,
                    Browser = "Concorde",
                    Device = "Concorde"
                }
            }
        };

        await this.Send(identify);
    }

    private void StartHandleMessages()
    {
        _ = Task.Run(this.HandleMessagesAsync, this._socketToken);
    }

    private async Task HandleMessagesAsync()
    {
        while (!this._socketToken.IsCancellationRequested)
        {
            var memoryOwner = MemoryOwner<byte>.Allocate(MinReceiveBufferSize);
            var memory = memoryOwner.Memory;

            if (this.Socket?.State != WebSocketState.Open)
            {
                await this.ResetSocket(true);
            }

            ValueWebSocketReceiveResult result;

            try
            {
                result = await this.Socket!.ReceiveAsync(memory, this._socketReceiveToken);
            }
            catch
            {
                memoryOwner.Dispose();
                
                continue;
            }

            var size = result.Count;

            if (!result.EndOfMessage)
            {
                var mediumMemoryOwner = MemoryOwner<byte>.Allocate(MediumReceiveBufferSize);
                var mediumMemory = mediumMemoryOwner.Memory;

                memory.CopyTo(mediumMemory);
                memoryOwner.Dispose();

                memoryOwner = mediumMemoryOwner;
                memory = mediumMemory;
                
                var freeMemory = memory[MinReceiveBufferSize..];

                result = await this.Socket.ReceiveAsync(freeMemory, this._socketToken);
                size += result.Count;
            }

            if (!result.EndOfMessage)
            {
                var largeMemoryOwner = MemoryOwner<byte>.Allocate(MaxReceiveBufferSize);
                var largeMemory = largeMemoryOwner.Memory;
                
                memory.CopyTo(largeMemory);
                memoryOwner.Dispose();

                memoryOwner = largeMemoryOwner;
                memory = largeMemory;

                var freeMemory = memory[MediumReceiveBufferSize..];

                result = await this.Socket.ReceiveAsync(freeMemory, this._socketToken);
                size += result.Count;
            }

            if (!result.EndOfMessage)
            {
                memoryOwner.Dispose();
                
                this._logger.LogError(
                    "Web socket message was over {Size} bytes and was discarded",
                    MaxReceiveBufferSize);

                continue;
            }

            if (this._socketToken.IsCancellationRequested)
            {
                return;
            }

            if (result.MessageType == WebSocketMessageType.Close)
            {
                // TODO: convert status to discord enum
                
                this._logger.LogWarning(
                    "Web socket was closed: {Reason} ({Code} - {Status})",
                    string.IsNullOrEmpty(this.Socket.CloseStatusDescription)
                        ? "[No description]"
                        : this.Socket.CloseStatusDescription,
                    (int) this.Socket.CloseStatus!,
                    this.Socket.CloseStatus);

                await this.ResetSocket();
                continue;
            }

            memoryOwner = memoryOwner[..size];

            _ = Task.Run(async () => await this.HandleMessageAsync(memoryOwner), this._socketToken)
                .ContinueWith(task =>
                {
                    if (!task.IsCompletedSuccessfully)
                    {
                        this._logger.LogError(task.Exception, "Failed to handle message");
                    }
                }, this._socketToken);
        }
    }

    private async Task HandleMessageAsync(MemoryOwner<byte> memoryOwner)
    {
        await using var stream = memoryOwner.AsStream();
        
        IDiscordSocketMessage? message = null;

        try
        {
            message = await JsonSerializer.DeserializeAsync<IDiscordSocketMessage>(
                stream,
                this._socketSerializerOptions,
                this._socketToken);
        }
        catch (JsonException)
        {
            // ignored
        }

        if (message == null)
        {
            this._logger.LogWarning("Received invalid Discord message");
            return;
        }

        var opcode = (Opcodes.Ids) message.Opcode;
        
        this._logger.LogDebug(
            "Received opcode {Id} - {Opcode} ({Size} bytes)",
            message.Opcode,
            opcode,
            memoryOwner.Length);

        switch (opcode)
        {
            case Opcodes.Ids.Dispatch:
            {
                var dispatchMessage = (BaseDispatchDiscordSocketMessage) message;
                this._lastSequenceNumber = dispatchMessage.SequenceNumber;

                await this.HandleDispatchAsync(dispatchMessage);
                
                break;
            }
            case Opcodes.Ids.Heartbeat:
            {
                await this.SendHeartbeat();
                
                this.RestartHeartbeat();
                
                break;
            }
            case Opcodes.Ids.Reconnect:
            {
                await this.ResetSocket(true);
                await this.SendResume();
                
                break;
            }
            case Opcodes.Ids.InvalidSession:
            {
                var invalidSessionData = ((InvalidSessionMessage) message).Data;
                
                this._logger.LogWarning("Session was invalidated");

                if (invalidSessionData)
                {
                    await this.ResetSocket(true);
                    await this.SendResume();
                }
                else
                {
                    await Task.Delay(Random.Shared.Next(1000, 5001), this._socketToken);
                    await this.ResetSocket();
                }
                
                break;
            }
            case Opcodes.Ids.Hello:
            {
                var helloData = ((HelloMessage) message).Data;
                
                this._logger.LogDebug(
                    "Got heartbeat interval {HeartbeatInterval}s",
                    helloData.HeartbeatInterval / 1000f);

                await this.SendHeartbeat();

                if (this._resuming)
                {
                    this._logger.LogTrace("Resuming");
                    
                    await this.SendResume();
                    this._resuming = false;
                }
                else
                {
                    this._logger.LogTrace("Identifying");
                    
                    await this.SendIdentify();
                }

                this.StartHeartbeat(helloData.HeartbeatInterval);
                
                break;
            }
            case Opcodes.Ids.HeartbeatAck:
            {
                this._heartbeatAcked = true;
                
                break;
            }
        }
    }

    private async Task SendHeartbeat()
    {
        var heartbeat = new HeartbeatMessage()
        {
            Data = this._lastSequenceNumber
        };

        await this.Send(heartbeat);
    }

    private async Task SendResume()
    {
        var resume = new ResumeMessage()
        {
            Data = new ResumeData()
            {
                Token = this._botToken,
                SessionId = this._sessionId!,
                LastSequenceNumber = this._lastSequenceNumber.GetValueOrDefault()
            }
        };

        await this.Send(resume);
    }

    private async Task ResetSocketAndWait()
    {
        this._logger.LogTrace("Resetting socket and waiting till ready");
        
        var readySource = new TaskCompletionSource();

        this.OnReady += _ =>
        {
            readySource.SetResult();
            return Task.CompletedTask;
        };

        await this.ResetSocket(true);

        await readySource.Task;
    }
}