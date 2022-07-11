using System.Net.Http.Json;
using Concorde.Abstractions.Schemas.Objects;
using Microsoft.Extensions.Logging;

namespace Concorde.Client;

public partial class BaseDiscordRestClient
{
    private const int RetryInterval = 5;
    
    public async Task<Gateway> GetGateway()
    {
        return await this.Get<Gateway>("gateway") ?? throw new Exception("Failed to get gateway URL");
    }

    protected async Task<T?> Get<T>(string requestUri)
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
}