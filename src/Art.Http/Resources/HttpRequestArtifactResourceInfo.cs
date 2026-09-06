using Art.Common;

namespace Art.Http.Resources;

/// <summary>
/// Provides artifact information.
/// </summary>
/// <param name="ArtifactTool">Artifact tool.</param>
/// <param name="RequestDelegate">A delegate that creates a new instance of <see cref="HttpRequestEx"/>.</param>
/// <param name="HttpRequestMetaConfig">Custom request configuration.</param>
/// <param name="Key">Resource key.</param>
/// <param name="ContentType">MIME content type.</param>
/// <param name="Updated">Date this resource was updated.</param>
/// <param name="Retrieved">Date this resource was retrieved.</param>
/// <param name="Version">Version.</param>
/// <param name="Checksum">Checksum.</param>
/// <param name="DynamicFileNameFunction">Function to use for transforming retrieved filename.</param>
public record HttpRequestArtifactResourceInfo(
    HttpArtifactTool ArtifactTool,
    Func<HttpRequestEx> RequestDelegate,
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
        await ArtifactTool.DownloadResourceAsync(RequestDelegate, targetStream, HttpRequestMetaConfig, exportOptions, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    /// <exception cref="TaskCanceledException">Thrown with <see cref="TimeoutException"/> <see cref="Exception.InnerException"/> for a timeout.</exception>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    /// <exception cref="ArtHttpResponseMessageException">Thrown on HTTP response indicating non-successful response.</exception>
    public override async ValueTask<Stream> GetStreamAsync(CancellationToken cancellationToken = default)
    {
        return await ArtifactTool.GetResourceDownloadStreamAsync(RequestDelegate, HttpRequestMetaConfig, cancellationToken).ConfigureAwait(false);
    }
}

public partial class HttpArtifactDataExtensions
{
    /// <summary>
    /// Creates a <see cref="HttpRequestArtifactResourceInfo"/> resource.
    /// </summary>
    /// <param name="artifactData">Source <see cref="ArtifactData"/> instance.</param>
    /// <param name="artifactTool">Artifact tool.</param>
    /// <param name="requestDelegate">A delegate that creates a new instance of <see cref="HttpRequestEx"/>.</param>
    /// <param name="key">Resource key.</param>
    /// <param name="contentType">MIME content type.</param>
    /// <param name="updated">Date this resource was updated.</param>
    /// <param name="retrieved">Date this resource was retrieved.</param>
    /// <param name="version">Version.</param>
    /// <param name="checksum">Checksum.</param>
    /// <param name="httpRequestMetaConfig">Custom request metaconfiguration.</param>
    /// <param name="dynamicFileNameFunction">Function to use for transforming retrieved filename.</param>
    public static ArtifactDataResource HttpRequest(this ArtifactData artifactData,
        HttpArtifactTool artifactTool,
        Func<HttpRequestEx> requestDelegate,
        ArtifactResourceKey key,
        string? contentType = "application/octet-stream",
        DateTimeOffset? updated = null,
        DateTimeOffset? retrieved = null,
        string? version = null,
        Checksum? checksum = null,
        HttpRequestMetaConfig? httpRequestMetaConfig = null,
        Func<string, string>? dynamicFileNameFunction = null)
        => new(artifactData, new HttpRequestArtifactResourceInfo(artifactTool, requestDelegate, httpRequestMetaConfig, key, contentType, updated, retrieved, version, checksum, dynamicFileNameFunction));

    /// <summary>
    /// Creates a <see cref="HttpRequestArtifactResourceInfo"/> resource.
    /// </summary>
    /// <param name="artifactData">Source <see cref="ArtifactData"/> instance.</param>
    /// <param name="artifactTool">Artifact tool.</param>
    /// <param name="requestDelegate">A delegate that creates a new instance of <see cref="HttpRequestEx"/>.</param>
    /// <param name="file">Filename.</param>
    /// <param name="path">Path.</param>
    /// <param name="contentType">MIME content type.</param>
    /// <param name="updated">Date this resource was updated.</param>
    /// <param name="retrieved">Date this resource was retrieved.</param>
    /// <param name="version">Version.</param>
    /// <param name="checksum">Checksum.</param>
    /// <param name="httpRequestMetaConfig">Custom request metaconfiguration.</param>
    /// <param name="dynamicFileNameFunction">Function to use for transforming retrieved filename.</param>
    public static ArtifactDataResource HttpRequest(this ArtifactData artifactData,
        HttpArtifactTool artifactTool,
        Func<HttpRequestEx> requestDelegate,
        string file,
        string path = "",
        string? contentType = "application/octet-stream",
        DateTimeOffset? updated = null,
        DateTimeOffset? retrieved = null,
        string? version = null,
        Checksum? checksum = null,
        HttpRequestMetaConfig? httpRequestMetaConfig = null,
        Func<string, string>? dynamicFileNameFunction = null)
        => new(artifactData, new HttpRequestArtifactResourceInfo(artifactTool, requestDelegate, httpRequestMetaConfig, new ArtifactResourceKey(artifactData.Info.Key, file, path), contentType, updated, retrieved, version, checksum, dynamicFileNameFunction));

    /// <summary>
    /// Creates a <see cref="HttpRequestArtifactResourceInfo"/> resource.
    /// </summary>
    /// <param name="artifactData">Source <see cref="ArtifactData"/> instance.</param>
    /// <param name="requestDelegate">A delegate that creates a new instance of <see cref="HttpRequestEx"/>.</param>
    /// <param name="key">Resource key.</param>
    /// <param name="contentType">MIME content type.</param>
    /// <param name="updated">Date this resource was updated.</param>
    /// <param name="retrieved">Date this resource was retrieved.</param>
    /// <param name="version">Version.</param>
    /// <param name="checksum">Checksum.</param>
    /// <param name="httpRequestMetaConfig">Custom request metaconfiguration.</param>
    /// <param name="dynamicFileNameFunction">Function to use for transforming retrieved filename.</param>
    public static ArtifactDataResource HttpRequest(this ArtifactData artifactData,
        Func<HttpRequestEx> requestDelegate,
        ArtifactResourceKey key,
        string? contentType = "application/octet-stream",
        DateTimeOffset? updated = null,
        DateTimeOffset? retrieved = null,
        string? version = null,
        Checksum? checksum = null,
        HttpRequestMetaConfig? httpRequestMetaConfig = null,
        Func<string, string>? dynamicFileNameFunction = null)
        => artifactData.HttpRequest(artifactData.GetArtifactTool<HttpArtifactTool>(), requestDelegate, key, contentType, updated, retrieved, version, checksum, httpRequestMetaConfig, dynamicFileNameFunction);

    /// <summary>
    /// Creates a <see cref="HttpRequestArtifactResourceInfo"/> resource.
    /// </summary>
    /// <param name="artifactData">Source <see cref="ArtifactData"/> instance.</param>
    /// <param name="requestDelegate">A delegate that creates a new instance of <see cref="HttpRequestEx"/>.</param>
    /// <param name="file">Filename.</param>
    /// <param name="path">Path.</param>
    /// <param name="contentType">MIME content type.</param>
    /// <param name="updated">Date this resource was updated.</param>
    /// <param name="retrieved">Date this resource was retrieved.</param>
    /// <param name="version">Version.</param>
    /// <param name="checksum">Checksum.</param>
    /// <param name="httpRequestMetaConfig">Custom request metaconfiguration.</param>
    /// <param name="dynamicFileNameFunction">Function to use for transforming retrieved filename.</param>
    public static ArtifactDataResource HttpRequest(this ArtifactData artifactData,
        Func<HttpRequestEx> requestDelegate,
        string file,
        string path = "",
        string? contentType = "application/octet-stream",
        DateTimeOffset? updated = null,
        DateTimeOffset? retrieved = null,
        string? version = null,
        Checksum? checksum = null,
        HttpRequestMetaConfig? httpRequestMetaConfig = null,
        Func<string, string>? dynamicFileNameFunction = null)
        => artifactData.HttpRequest(artifactData.GetArtifactTool<HttpArtifactTool>(), requestDelegate, file, path, contentType, updated, retrieved, version, checksum, httpRequestMetaConfig, dynamicFileNameFunction);
}
