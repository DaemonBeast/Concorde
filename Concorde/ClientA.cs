/*using System.Net.Http.Json;
using System.Net.WebSockets;
using Concorde.Schemas.Rest;
using Microsoft.Extensions.Logging;

namespace Concorde;

public class ClientA : ClientBase
{
    public ClientA(ILogger<Client> logger, HttpClient httpClient, ClientWebSocket webSocket)
        : base(logger, httpClient, webSocket)
    {
        this.OnReady += async () =>
        {
            this.Logger.LogInformation("a");
            this.Logger.LogInformation("Username: {Username}", (await this.GetCurrentUser()).Username);
        };
    }

    public async Task<User> GetCurrentUser()
    {
        return await this.HttpClient.GetFromJsonAsync<User>("user/@me")
               ?? throw new Exception("Failed to get current user");
    }
}*/