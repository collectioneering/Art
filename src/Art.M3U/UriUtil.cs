using System.Collections.Specialized;
using System.Web;

namespace Art.M3U;

internal static class UriUtil
{
    public static Uri CombineUri(Uri uri, string path)
    {
        string query = uri.Query;
        var queryContent = string.IsNullOrEmpty(query) ? new NameValueCollection() : HttpUtility.ParseQueryString(query);
        if (path.IndexOf('?') is var pathQueryIndex and >= 0)
        {
            var pathQueryContent = HttpUtility.ParseQueryString(path[(pathQueryIndex + 1)..]);
            foreach (string? key in pathQueryContent.AllKeys)
            {
                if (key == null)
                {
                    continue;
                }
                queryContent.Set(key, pathQueryContent.Get(key));
            }
        }
        string combinedQuery = string.Join("&", queryContent.AllKeys.Select(a => $"{a}={HttpUtility.UrlEncode(queryContent[a])}"));
        return new UriBuilder(new Uri(uri, path)) { Query = combinedQuery }.Uri;
    }
}
