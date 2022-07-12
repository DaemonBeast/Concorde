using System.Net.Http.Json;
using Concorde.Abstractions;
using Concorde.Abstractions.Schemas.Objects;
using Concorde.Abstractions.Schemas.Rest;
using Microsoft.Extensions.Logging;

namespace Concorde.Client;

public partial class BaseDiscordRestClient
{
    private const int RetryInterval = 5;
    
    public async Task<T?> Get<T>(string requestUri)
    {
        this._logger.LogTrace("Sending HTTP GET request to {RequestUri}", requestUri);
        
        while (!this._restToken.IsCancellationRequested)
        {
            try
            {
                return await this.HttpClient.GetFromJsonAsync<T>(requestUri, this._restToken);
            }
            catch
            {
                // ignored
            }

            try
            {
                this._logger.LogTrace("HTTP GET request failed. Retrying after {RetryInterval} seconds", RetryInterval);
                
                await Task.Delay(TimeSpan.FromSeconds(RetryInterval), this._restToken);
            }
            catch
            {
                // ignored
            }
        }

        throw new Exception();
    }
    
    public async Task<T?> Post<T, TData>(string requestUri, TData data) where TData : IDiscordRestObject
    {
        this._logger.LogTrace("Sending HTTP POST request to {RequestUri}", requestUri);
        
        while (!this._restToken.IsCancellationRequested)
        {
            try
            {
                return await (await this.HttpClient.PostAsJsonAsync(requestUri, data, this._restToken)).Content
                    .ReadFromJsonAsync<T>(cancellationToken: this._restToken);
            }
            catch
            {
                // ignored
            }

            try
            {
                this._logger.LogTrace(
                    "HTTP POST request failed. Retrying after {RetryInterval} seconds",
                    RetryInterval);
                
                await Task.Delay(TimeSpan.FromSeconds(RetryInterval), this._restToken);
            }
            catch
            {
                // ignored
            }
        }

        throw new Exception();
    }
    
    public async Task<Gateway> GetGateway()
    {
        return await this.Get<Gateway>("gateway") ?? throw new Exception("Failed to get gateway URL");
    }

    public async Task<Message> SendMessage(MessageCreate messageCreate, Snowflake channelId)
    {
        return await this.Post<Message, MessageCreate>($"channels/{channelId}/messages", messageCreate)
            ?? throw new Exception("Failed to send message");
    }
}