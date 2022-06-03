using System.Net.Http.Headers;
using System.Net.WebSockets;
using Concorde.Utilities;

namespace Concorde;

public class ClientBase
{
    private readonly HttpClient _httpClient;
    private readonly WebSocket _webSocket;

    private const string BaseUrl = "https://discord.com/api";
    private const int ApiVersion = 10;

    private static readonly string UserAgent =
        $"DiscordBot ({Constants.ProjectLink}, {DotnetUtilities.Version}) .NET-CoreCLR/{Environment.Version}";

    public ClientBase(string token, TokenType tokenType = TokenType.Bot)
    {
        this._httpClient = new HttpClient();
        this.ConfigureHttpClient(token, tokenType);

        this._webSocket = new ClientWebSocket();
    }

    private void ConfigureHttpClient(string token, TokenType tokenType)
    {
        this._httpClient.BaseAddress = GetApiUrl();
        
        var headers = this._httpClient.DefaultRequestHeaders;
        headers.Authorization = new AuthenticationHeaderValue(tokenType.ToString(), token);
        headers.UserAgent.ParseAdd(UserAgent);
    }

    private static Uri GetApiUrl()
    {
        return new Uri(Path.Join(BaseUrl, $"v{ApiVersion}"));
    }
}

public enum TokenType
{
    Bot,
    Bearer
}

public enum GatewayEncoding
{
    Json,
    // Etf
}