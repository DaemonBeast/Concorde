using System.Collections.Specialized;
using System.Web;

namespace Concorde.Extensions;

public static class HttpExtensions
{
    public static Uri AddQueryParameters(this Uri uri, NameValueCollection queryParameters)
    {
        var uriBuilder = new UriBuilder(uri);
        
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query.Add(queryParameters);

        uriBuilder.Query = query.ToString();

        return uriBuilder.Uri;
    }
}