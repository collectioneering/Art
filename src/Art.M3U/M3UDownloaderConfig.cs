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
    M3UTiming? Timing = null);
