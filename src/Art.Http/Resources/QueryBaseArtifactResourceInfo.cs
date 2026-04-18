using System.Text.RegularExpressions;
using System.Web;

namespace Art.Http.Resources;

/// <summary>
/// Provides artifact information.
/// </summary>
/// <param name="Key">Resource key.</param>
/// <param name="ContentType">MIME content type.</param>
/// <param name="Updated">Date this resource was updated.</param>
/// <param name="Retrieved">Date this resource was retrieved.</param>
/// <param name="Version">Version.</param>
/// <param name="Checksum">Checksum.</param>
/// <param name="DynamicFileNameFunction">Function to use for transforming retrieved filename.</param>
/// <param name="ContentLength">Content length.</param>
public partial record QueryBaseArtifactResourceInfo(
    ArtifactResourceKey Key,
    string? ContentType = "application/octet-stream",
    DateTimeOffset? Updated = null,
    DateTimeOffset? Retrieved = null,
    string? Version = null,
    Checksum? Checksum = null,
    Func<string, string>? DynamicFileNameFunction = null,
    long? ContentLength = null)
    : ArtifactResourceInfo(Key, ContentType, Updated, Retrieved, Version, Checksum)
{
    [GeneratedRegex(@"^attachment; filename\*=UTF-8''(?<nameUrlEncoded>\S+)$")]
    private static partial Regex GetContentDispositionRegex();


    [GeneratedRegex("""^attachment; filename="(?<name>\S+)"$""")]
    private static partial Regex GetContentDispositionRegexAlternate();

    /// <summary>
    /// Gets this instance modified with metadata from specified response.
    /// </summary>
    /// <param name="response">Response.</param>
    /// <returns>Instance.</returns>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    /// <exception cref="ArtHttpResponseMessageException">Thrown on HTTP response indicating non-successful response.</exception>
    protected ArtifactResourceInfo WithMetadata(HttpResponseMessage response)
    {
        ArtHttpResponseMessageException.EnsureSuccessStatusCode(response);
        string? contentType = response.Content.Headers.ContentType?.MediaType;
        DateTimeOffset? updated = response.Content.Headers.LastModified;
        string? version = response.Headers.ETag?.Tag;
        long? contentLength = response.Content.Headers.ContentLength;
        string? fileName = null;
        if (DynamicFileNameFunction != null
            && response.Content.Headers.TryGetValues("Content-Disposition", out var contentDispositionValues)
            && contentDispositionValues.LastOrDefault() is { } contentDispositionValue)
        {
            if (GetContentDispositionRegex().Match(contentDispositionValue) is { Success: true } fileNameMatch)
            {
                fileName = DynamicFileNameFunction(HttpUtility.UrlDecode(fileNameMatch.Groups["nameUrlEncoded"].Value));
            }
            else if (GetContentDispositionRegexAlternate().Match(contentDispositionValue) is { Success: true } fileNameMatchAlternate)
            {
                fileName = DynamicFileNameFunction(fileNameMatchAlternate.Groups["name"].Value);
            }
        }
        ArtifactResourceKey key = fileName != null ? Key with { File = fileName } : Key;
        return this with
        {
            Key = key,
            ContentType = contentType ?? ContentType,
            Updated = updated ?? Updated,
            Version = version ?? Version,
            ContentLength = contentLength ?? ContentLength
        };
    }

    /// <inheritdoc />
    public override void AugmentOutputStreamOptions(ref OutputStreamOptions options)
    {
        if (ContentLength is { } contentLength)
        {
            options = options with { PreallocationSize = Math.Clamp(contentLength, 0, MaxStreamDownloadPreallocationSize) };
        }
    }

    /// <summary>
    /// Maximum preallocation size for direct downloads.
    /// </summary>
    public const long MaxStreamDownloadPreallocationSize = 256 * 1024 * 1024;
}
