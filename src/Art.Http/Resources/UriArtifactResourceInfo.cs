using Art.Common;

namespace Art.Http.Resources;

/// <summary>
/// Provides artifact information.
/// </summary>
/// <param name="ArtifactTool">Artifact tool.</param>
/// <param name="Uri">URI.</param>
/// <param name="Key">Resource key.</param>
/// <param name="ContentType">MIME content type.</param>
/// <param name="Updated">Date this resource was updated.</param>
/// <param name="Retrieved">Date this resource was retrieved.</param>
/// <param name="Version">Version.</param>
/// <param name="Checksum">Checksum.</param>
/// <param name="HttpRequestConfigDelegate">Delegate to create custom request configuration.</param>
/// <param name="HttpRequestMetaConfig">Custom request configuration.</param>
/// <param name="DynamicFileNameFunction">Function to use for transforming retrieved filename.</param>
public record UriArtifactResourceInfo(
    HttpArtifactTool ArtifactTool,
    Uri Uri,
    Func<HttpRequestConfig?>? HttpRequestConfigDelegate,
    HttpRequestMetaConfig? HttpRequestMetaConfig,
    ArtifactResourceKey Key,
    string? ContentType = "application/octet-stream",
    DateTimeOffset? Updated = null,
    DateTimeOffset? Retrieved = null,
    string? Version = null,
    Checksum? Checksum = null,
    Func<string, string>? DynamicFileNameFunction = null)
    : QueryBaseArtifactResourceInfo(Key, ContentType, Updated, Retrieved, Version, Checksum, DynamicFileNameFunction)
{
    /// <inheritdoc/>
    public override bool CanExportStream => true;

    /// <inheritdoc />
    public override bool CanGetStream => true;

    /// <inheritdoc/>
    /// <exception cref="TaskCanceledException">Thrown with <see cref="TimeoutException"/> <see cref="Exception.InnerException"/> for a timeout.</exception>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    /// <exception cref="ArtHttpResponseMessageException">Thrown on HTTP response indicating non-successful response.</exception>
    public override async ValueTask ExportStreamAsync(Stream targetStream, ArtifactResourceExportOptions? exportOptions = null, CancellationToken cancellationToken = default)
    {
        // M3U behaviour depends on calling this member, or any overload targeting the contained HttpClient. Don't change this.
        await ArtifactTool.DownloadResourceAsync(Uri, targetStream, HttpRequestConfigDelegate, HttpRequestMetaConfig, exportOptions, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    /// <exception cref="TaskCanceledException">Thrown with <see cref="TimeoutException"/> <see cref="Exception.InnerException"/> for a timeout.</exception>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    /// <exception cref="ArtHttpResponseMessageException">Thrown on HTTP response indicating non-successful response.</exception>
    public override async ValueTask<Stream> GetStreamAsync(CancellationToken cancellationToken = default)
    {
        return await ArtifactTool.GetResourceDownloadStreamAsync(Uri, HttpRequestConfigDelegate, HttpRequestMetaConfig, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override bool UsesMetadata => true;

    /// <inheritdoc/>
    /// <exception cref="TaskCanceledException">Thrown with <see cref="TimeoutException"/> <see cref="Exception.InnerException"/> for a timeout.</exception>
    public override async ValueTask<ArtifactResourceInfo> WithMetadataAsync(CancellationToken cancellationToken = default)
    {
        HttpResponseMessage res = await ArtifactTool.HeadAsync(Uri, HttpRequestConfigDelegate, HttpRequestMetaConfig, cancellationToken).ConfigureAwait(false);
        ArtHttpResponseMessageException.EnsureSuccessStatusCode(res);
        return WithMetadata(res);
    }
}

public partial class HttpArtifactDataExtensions
{
    /// <summary>
    /// Creates a <see cref="UriArtifactResourceInfo"/> resource.
    /// </summary>
    /// <param name="artifactData">Source <see cref="ArtifactData"/> instance.</param>
    /// <param name="artifactTool">Artifact tool.</param>
    /// <param name="uri">URI.</param>
    /// <param name="key">Resource key.</param>
    /// <param name="contentType">MIME content type.</param>
    /// <param name="updated">Date this resource was updated.</param>
    /// <param name="retrieved">Date this resource was retrieved.</param>
    /// <param name="version">Version.</param>
    /// <param name="checksum">Checksum.</param>
    /// <param name="httpRequestConfigDelegate">Delegate to create custom request configuration.</param>
    /// <param name="httpRequestMetaConfig">Custom request configuration.</param>
    /// <param name="dynamicFileNameFunction">Function to use for transforming retrieved filename.</param>
    public static ArtifactDataResource Uri(this ArtifactData artifactData,
        HttpArtifactTool artifactTool,
        Uri uri,
        ArtifactResourceKey key,
        string? contentType = "application/octet-stream",
        DateTimeOffset? updated = null,
        DateTimeOffset? retrieved = null,
        string? version = null,
        Checksum? checksum = null,
        Func<HttpRequestConfig?>? httpRequestConfigDelegate = null,
        HttpRequestMetaConfig? httpRequestMetaConfig = null,
        Func<string, string>? dynamicFileNameFunction = null)
        => new(
            artifactData,
            new UriArtifactResourceInfo(artifactTool,
                uri,
                httpRequestConfigDelegate,
                httpRequestMetaConfig,
                key,
                contentType,
                updated,
                retrieved,
                version,
                checksum,
                dynamicFileNameFunction));

    /// <summary>
    /// Creates a <see cref="UriArtifactResourceInfo"/> resource.
    /// </summary>
    /// <param name="artifactData">Source <see cref="ArtifactData"/> instance.</param>
    /// <param name="artifactTool">Artifact tool.</param>
    /// <param name="uri">URI.</param>
    /// <param name="file">Filename.</param>
    /// <param name="path">Path.</param>
    /// <param name="contentType">MIME content type.</param>
    /// <param name="updated">Date this resource was updated.</param>
    /// <param name="retrieved">Date this resource was retrieved.</param>
    /// <param name="version">Version.</param>
    /// <param name="checksum">Checksum.</param>
    /// <param name="httpRequestConfigDelegate">Delegate to create custom request configuration.</param>
    /// <param name="httpRequestMetaConfig">Custom request configuration.</param>
    /// <param name="dynamicFileNameFunction">Function to use for transforming retrieved filename.</param>
    public static ArtifactDataResource Uri(this ArtifactData artifactData,
        HttpArtifactTool artifactTool,
        Uri uri,
        string file,
        string path = "",
        string? contentType = "application/octet-stream",
        DateTimeOffset? updated = null,
        DateTimeOffset? retrieved = null,
        string? version = null,
        Checksum? checksum = null,
        Func<HttpRequestConfig?>? httpRequestConfigDelegate = null,
        HttpRequestMetaConfig? httpRequestMetaConfig = null,
        Func<string, string>? dynamicFileNameFunction = null)
        => new(
            artifactData,
            new UriArtifactResourceInfo(artifactTool,
                uri,
                httpRequestConfigDelegate,
                httpRequestMetaConfig,
                new ArtifactResourceKey(artifactData.Info.Key,
                    file,
                    path),
                contentType,
                updated,
                retrieved,
                version,
                checksum,
                dynamicFileNameFunction));

    /// <summary>
    /// Creates a <see cref="UriArtifactResourceInfo"/> resource.
    /// </summary>
    /// <param name="artifactData">Source <see cref="ArtifactData"/> instance.</param>
    /// <param name="uri">URI.</param>
    /// <param name="key">Resource key.</param>
    /// <param name="contentType">MIME content type.</param>
    /// <param name="updated">Date this resource was updated.</param>
    /// <param name="retrieved">Date this resource was retrieved.</param>
    /// <param name="version">Version.</param>
    /// <param name="checksum">Checksum.</param>
    /// <param name="httpRequestConfigDelegate">Delegate to create custom request configuration.</param>
    /// <param name="httpRequestMetaConfig">Custom request configuration.</param>
    /// <param name="dynamicFileNameFunction">Function to use for transforming retrieved filename.</param>
    public static ArtifactDataResource Uri(this ArtifactData artifactData,
        Uri uri,
        ArtifactResourceKey key,
        string? contentType = "application/octet-stream",
        DateTimeOffset? updated = null,
        DateTimeOffset? retrieved = null,
        string? version = null,
        Checksum? checksum = null,
        Func<HttpRequestConfig?>? httpRequestConfigDelegate = null,
        HttpRequestMetaConfig? httpRequestMetaConfig = null,
        Func<string, string>? dynamicFileNameFunction = null)
        => artifactData.Uri(
            artifactData.GetArtifactTool<HttpArtifactTool>(),
            uri,
            key,
            contentType,
            updated,
            retrieved,
            version,
            checksum,
            httpRequestConfigDelegate,
            httpRequestMetaConfig,
            dynamicFileNameFunction);

    /// <summary>
    /// Creates a <see cref="UriArtifactResourceInfo"/> resource.
    /// </summary>
    /// <param name="artifactData">Source <see cref="ArtifactData"/> instance.</param>
    /// <param name="uri">URI.</param>
    /// <param name="file">Filename.</param>
    /// <param name="path">Path.</param>
    /// <param name="contentType">MIME content type.</param>
    /// <param name="updated">Date this resource was updated.</param>
    /// <param name="retrieved">Date this resource was retrieved.</param>
    /// <param name="version">Version.</param>
    /// <param name="checksum">Checksum.</param>
    /// <param name="httpRequestConfigDelegate">Delegate to create custom request configuration.</param>
    /// <param name="httpRequestMetaConfig">Custom request configuration.</param>
    /// <param name="dynamicFileNameFunction">Function to use for transforming retrieved filename.</param>
    public static ArtifactDataResource Uri(this ArtifactData artifactData,
        Uri uri,
        string file,
        string path = "",
        string? contentType = "application/octet-stream",
        DateTimeOffset? updated = null,
        DateTimeOffset? retrieved = null,
        string? version = null,
        Checksum? checksum = null,
        Func<HttpRequestConfig?>? httpRequestConfigDelegate = null,
        HttpRequestMetaConfig? httpRequestMetaConfig = null,
        Func<string, string>? dynamicFileNameFunction = null)
        => artifactData.Uri(
            artifactData.GetArtifactTool<HttpArtifactTool>(),
            uri,
            file,
            path,
            contentType,
            updated,
            retrieved,
            version,
            checksum,
            httpRequestConfigDelegate,
            httpRequestMetaConfig,
            dynamicFileNameFunction);

    /// <summary>
    /// Creates a <see cref="UriArtifactResourceInfo"/> resource.
    /// </summary>
    /// <param name="artifactData">Source <see cref="ArtifactData"/> instance.</param>
    /// <param name="artifactTool">Artifact tool.</param>
    /// <param name="uri">URI.</param>
    /// <param name="key">Resource key.</param>
    /// <param name="contentType">MIME content type.</param>
    /// <param name="updated">Date this resource was updated.</param>
    /// <param name="retrieved">Date this resource was retrieved.</param>
    /// <param name="version">Version.</param>
    /// <param name="checksum">Checksum.</param>
    /// <param name="httpRequestConfigDelegate">Delegate to create custom request configuration.</param>
    /// <param name="httpRequestMetaConfig">Custom request configuration.</param>
    /// <param name="dynamicFileNameFunction">Function to use for transforming retrieved filename.</param>
    public static ArtifactDataResource Uri(this ArtifactData artifactData,
        HttpArtifactTool artifactTool,
        string uri,
        ArtifactResourceKey key,
        string? contentType = "application/octet-stream",
        DateTimeOffset? updated = null,
        DateTimeOffset? retrieved = null,
        string? version = null,
        Checksum? checksum = null,
        Func<HttpRequestConfig?>? httpRequestConfigDelegate = null,
        HttpRequestMetaConfig? httpRequestMetaConfig = null,
        Func<string, string>? dynamicFileNameFunction = null)
        => new(
            artifactData,
            new UriArtifactResourceInfo(artifactTool,
                new Uri(uri),
                httpRequestConfigDelegate,
                httpRequestMetaConfig,
                key,
                contentType,
                updated,
                retrieved,
                version,
                checksum,
                dynamicFileNameFunction));

    /// <summary>
    /// Creates a <see cref="UriArtifactResourceInfo"/> resource.
    /// </summary>
    /// <param name="artifactData">Source <see cref="ArtifactData"/> instance.</param>
    /// <param name="artifactTool">Artifact tool.</param>
    /// <param name="uri">URI.</param>
    /// <param name="file">Filename.</param>
    /// <param name="path">Path.</param>
    /// <param name="contentType">MIME content type.</param>
    /// <param name="updated">Date this resource was updated.</param>
    /// <param name="retrieved">Date this resource was retrieved.</param>
    /// <param name="version">Version.</param>
    /// <param name="checksum">Checksum.</param>
    /// <param name="httpRequestConfigDelegate">Delegate to create custom request configuration.</param>
    /// <param name="httpRequestMetaConfig">Custom request configuration.</param>
    /// <param name="dynamicFileNameFunction">Function to use for transforming retrieved filename.</param>
    public static ArtifactDataResource Uri(this ArtifactData artifactData,
        HttpArtifactTool artifactTool,
        string uri,
        string file,
        string path = "",
        string? contentType = "application/octet-stream",
        DateTimeOffset? updated = null,
        DateTimeOffset? retrieved = null,
        string? version = null,
        Checksum? checksum = null,
        Func<HttpRequestConfig?>? httpRequestConfigDelegate = null,
        HttpRequestMetaConfig? httpRequestMetaConfig = null,
        Func<string, string>? dynamicFileNameFunction = null)
        => new(
            artifactData,
            new UriArtifactResourceInfo(artifactTool,
                new Uri(uri),
                httpRequestConfigDelegate,
                httpRequestMetaConfig,
                new ArtifactResourceKey(artifactData.Info.Key,
                    file,
                    path),
                contentType,
                updated,
                retrieved,
                version,
                checksum,
                dynamicFileNameFunction));

    /// <summary>
    /// Creates a <see cref="UriArtifactResourceInfo"/> resource.
    /// </summary>
    /// <param name="artifactData">Source <see cref="ArtifactData"/> instance.</param>
    /// <param name="uri">URI.</param>
    /// <param name="key">Resource key.</param>
    /// <param name="contentType">MIME content type.</param>
    /// <param name="updated">Date this resource was updated.</param>
    /// <param name="retrieved">Date this resource was retrieved.</param>
    /// <param name="version">Version.</param>
    /// <param name="checksum">Checksum.</param>
    /// <param name="httpRequestConfigDelegate">Delegate to create custom request configuration.</param>
    /// <param name="httpRequestMetaConfig">Custom request configuration.</param>
    /// <param name="dynamicFileNameFunction">Function to use for transforming retrieved filename.</param>
    public static ArtifactDataResource Uri(this ArtifactData artifactData,
        string uri,
        ArtifactResourceKey key,
        string? contentType = "application/octet-stream",
        DateTimeOffset? updated = null,
        DateTimeOffset? retrieved = null,
        string? version = null,
        Checksum? checksum = null,
        Func<HttpRequestConfig?>? httpRequestConfigDelegate = null,
        HttpRequestMetaConfig? httpRequestMetaConfig = null,
        Func<string, string>? dynamicFileNameFunction = null)
        => artifactData.Uri(
            artifactData.GetArtifactTool<HttpArtifactTool>(),
            uri,
            key,
            contentType,
            updated,
            retrieved,
            version,
            checksum,
            httpRequestConfigDelegate,
            httpRequestMetaConfig,
            dynamicFileNameFunction);

    /// <summary>
    /// Creates a <see cref="UriArtifactResourceInfo"/> resource.
    /// </summary>
    /// <param name="artifactData">Source <see cref="ArtifactData"/> instance.</param>
    /// <param name="uri">URI.</param>
    /// <param name="file">Filename.</param>
    /// <param name="path">Path.</param>
    /// <param name="contentType">MIME content type.</param>
    /// <param name="updated">Date this resource was updated.</param>
    /// <param name="retrieved">Date this resource was retrieved.</param>
    /// <param name="version">Version.</param>
    /// <param name="checksum">Checksum.</param>
    /// <param name="httpRequestConfigDelegate">Delegate to create custom request configuration.</param>
    /// <param name="httpRequestMetaConfig">Custom request configuration.</param>
    /// <param name="dynamicFileNameFunction">Function to use for transforming retrieved filename.</param>
    public static ArtifactDataResource Uri(this ArtifactData artifactData,
        string uri,
        string file,
        string path = "",
        string? contentType = "application/octet-stream",
        DateTimeOffset? updated = null,
        DateTimeOffset? retrieved = null,
        string? version = null,
        Checksum? checksum = null,
        Func<HttpRequestConfig?>? httpRequestConfigDelegate = null,
        HttpRequestMetaConfig? httpRequestMetaConfig = null,
        Func<string, string>? dynamicFileNameFunction = null)
        => artifactData.Uri(
            artifactData.GetArtifactTool<HttpArtifactTool>(),
            uri,
            file,
            path,
            contentType,
            updated,
            retrieved,
            version,
            checksum,
            httpRequestConfigDelegate,
            httpRequestMetaConfig,
            dynamicFileNameFunction);
}
