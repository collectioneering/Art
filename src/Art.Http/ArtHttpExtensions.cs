namespace Art.Http;

/// <summary>
/// Provides utility functions.
/// </summary>
public static class ArtHttpExtensions
{
    /// <summary>
    /// Sets origin and referrer on a request.
    /// </summary>
    /// <param name="request">Request to configure.</param>
    /// <param name="origin">Request origin.</param>
    /// <param name="referrer">Request referrer.</param>
    public static void SetOriginAndReferrer(this HttpRequestMessage request, string? origin, string? referrer)
    {
        if (referrer != null)
        {
            if (origin != null)
            {
                request.Headers.Add("origin", origin);
                request.Headers.Referrer = new Uri(referrer);
                return;
            }
            SetOriginAndReferrer(request, new Uri(referrer));
        }
        else if (origin != null)
        {
            SetOriginAndReferrer(request, new Uri(origin));
        }
    }

    private static void SetOriginAndReferrer(HttpRequestMessage request, Uri uri)
    {
        request.Headers.Add("origin", uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.UriEscaped));
        request.Headers.Referrer = uri;
    }
}
