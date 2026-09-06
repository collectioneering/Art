using Art.Http;

namespace Art.M3U;

/// <summary>
/// Represents configuration for downloader.
/// </summary>
/// <param name="UrlDelegate">Base stream URL.</param>
/// <param name="ArtifactKey">Base artifact key.</param>
/// <param name="SkipExistingSegments">Skip registered segments.</param>
/// <param name="Decrypt">Decrypt data inline.</param>
/// <param name="PrioritizeResolution">Prioritize resolution in stream selection.</param>
/// <param name="MaxConsecutiveRetries">Maximum allowed consecutive retries to perform.</param>
/// <param name="MaxTotalRetries">Maximum allowed total retries to perform.</param>
/// <param name="Referrer">Stream download referrer.</param>
/// <param name="Origin">Stream download origin.</param>
/// <param name="Headers">Headers to add to each request.</param>
/// <param name="Timing">Timing values.</param>
public record M3UDownloaderConfig(
    Func<string> UrlDelegate,
    ArtifactKey ArtifactKey,
    bool SkipExistingSegments = true,
    bool Decrypt = false,
    bool PrioritizeResolution = false,
    int? MaxConsecutiveRetries = 1,
    int? MaxTotalRetries = null,
    string? Referrer = null,
    string? Origin = null,
    IReadOnlyCollection<KeyValuePair<string, string>>? Headers = null,
    M3UTiming? Timing = null)
{
    /// <summary>
    /// Initializes an instance of <see cref="HttpRequestConfig"/>.
    /// </summary>
    /// <returns></returns>
    public HttpRequestConfig CreateHttpRequestConfig()
    {
        // TODO dynamic referrer/origin?
        return new HttpRequestConfig(Referrer: Referrer, Origin: Origin, RequestAction: CreateRequestAction(this));
    }

    private static Action<HttpRequestMessage>? CreateRequestAction(M3UDownloaderConfig config)
    {
        if (config.Headers == null)
        {
            return null;
        }
        return SetupRequest;

        void SetupRequest(HttpRequestMessage hrm)
        {
            foreach (var v in config.Headers)
            {
                hrm.Headers.Add(v.Key, v.Value);
            }
        }
    }
}
