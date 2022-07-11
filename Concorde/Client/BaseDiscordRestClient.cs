using System.Net.Http.Headers;
using Concorde.Abstractions.Client;
using Concorde.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Concorde.Client;

public partial class BaseDiscordRestClient : IDiscordRestClient
{
    protected readonly HttpClient HttpClient;

    private ILogger<BaseDiscordRestClient> _logger;
    private readonly IConfiguration _configuration;
    
    private readonly CancellationTokenSource _restTokenSource;
    private readonly CancellationToken _restToken;
    
    private readonly string _botToken;

    public BaseDiscordRestClient(
        HttpClient httpClient,
        ILogger<BaseDiscordRestClient> logger,
        IConfiguration configuration)
    {
        this.HttpClient = httpClient;
        
        this._logger = logger;
        this._configuration = configuration;

        this._restTokenSource = new CancellationTokenSource();
        this._restToken = this._restTokenSource.Token;
        
        this._botToken = this._configuration.GetValue<string>("token") ??
                         throw new Exception(
                             "Bot token must be specified via the command line or environment variables");
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        this.HttpClient.BaseAddress = DiscordApiUtilities.GetApiUrl();

        var headers = this.HttpClient.DefaultRequestHeaders;
        headers.Authorization = new AuthenticationHeaderValue("Bot", this._botToken);
        headers.UserAgent.ParseAdd(DiscordApiUtilities.GetUserAgent());
        
        this._logger.LogTrace("Started Discord REST client");

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        this._restTokenSource.Cancel();

        this.HttpClient.CancelPendingRequests();
        
        this._logger.LogTrace("Stopped Discord REST client");

        return Task.CompletedTask;
    }
}