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
/// <param name="HttpRequestConfigDelegate">Delegate to create custom request configuration.</param>
/// <param name="Timing">Timing values.</param>
public record M3UDownloaderConfig(
    Func<string> UrlDelegate,
    ArtifactKey ArtifactKey,
    bool SkipExistingSegments = true,
    bool Decrypt = false,
    bool PrioritizeResolution = false,
    int? MaxConsecutiveRetries = 1,
    int? MaxTotalRetries = null,
    Func<HttpRequestConfig?>? HttpRequestConfigDelegate = null,
    M3UTiming? Timing = null);
