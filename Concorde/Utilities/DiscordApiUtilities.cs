using System.Collections.Specialized;
using Concorde.Extensions;

namespace Concorde.Utilities;

public static class DiscordApiUtilities
{
    public static string GetUserAgent()
    {
        return $"DiscordBot ({Constants.Project.Link}, {DotnetUtilities.Version})";
    }
    
    public static Uri GetApiUrl()
    {
        return new Uri(Path.Combine(Constants.Discord.Api.BaseHttpUrl, $"v{Constants.Discord.Api.Version}"));
    }

    public static Uri GetGatewayUrl(string baseUrl)
    {
        return new Uri(baseUrl).AddQueryParameters(
            new NameValueCollection()
            {
                { "v", Constants.Discord.Api.Version.ToString() },
                { "encoding", "json" }
            });
    }
}