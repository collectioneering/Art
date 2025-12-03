using Art.Common;

namespace Art.Http.Resources;

/// <summary>
/// Provides artifact information.
/// </summary>
/// <param name="ArtifactTool">Artifact tool.</param>
/// <param name="Request">Request.</param>
/// <param name="HttpRequestConfig">Custom request configuration.</param>
/// <param name="Key">Resource key.</param>
/// <param name="ContentType">MIME content type.</param>
/// <param name="Updated">Date this resource was updated.</param>
/// <param name="Retrieved">Date this resource was retrieved.</param>
/// <param name="Version">Version.</param>
/// <param name="Checksum">Checksum.</param>
/// <param name="DynamicFileNameFunction">Function to use for transforming retrieved filename.</param>
public record HttpRequestMessageArtifactResourceInfo(
    HttpArtifactTool ArtifactTool,
    HttpRequestMessage Request,
    HttpRequestConfig? HttpRequestConfig,
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
    public override async ValueTask ExportStreamAsync(Stream targetStream, bool useLogger = true, CancellationToken cancellationToken = default)
    {
        await ArtifactTool.DownloadResourceAsync(Request, targetStream, HttpRequestConfig, useLogger, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    /// <exception cref="TaskCanceledException">Thrown with <see cref="TimeoutException"/> <see cref="Exception.InnerException"/> for a timeout.</exception>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    /// <exception cref="ArtHttpResponseMessageException">Thrown on HTTP response indicating non-successful response.</exception>
    public override async ValueTask<Stream> GetStreamAsync(CancellationToken cancellationToken = default)
    {
        return await ArtifactTool.GetResourceDownloadStreamAsync(Request, HttpRequestConfig, cancellationToken).ConfigureAwait(false);
    }
}

public partial class HttpArtifactDataExtensions
{
    /// <summary>
    /// Creates a <see cref="HttpRequestMessageArtifactResourceInfo"/> resource.
    /// </summary>
    /// <param name="artifactData">Source <see cref="ArtifactData"/> instance.</param>
    /// <param name="artifactTool">Artifact tool.</param>
    /// <param name="request">Request.</param>
    /// <param name="key">Resource key.</param>
    /// <param name="contentType">MIME content type.</param>
    /// <param name="updated">Date this resource was updated.</param>
    /// <param name="retrieved">Date this resource was retrieved.</param>
    /// <param name="version">Version.</param>
    /// <param name="checksum">Checksum.</param>
    /// <param name="httpRequestConfig">Custom request configuration.</param>
    /// <param name="dynamicFileNameFunction">Function to use for transforming retrieved filename.</param>
    public static ArtifactDataResource HttpRequestMessage(this ArtifactData artifactData,
        HttpArtifactTool artifactTool,
        HttpRequestMessage request,
        ArtifactResourceKey key,
        string? contentType = "application/octet-stream",
        DateTimeOffset? updated = null,
        DateTimeOffset? retrieved = null,
        string? version = null,
        Checksum? checksum = null,
        HttpRequestConfig? httpRequestConfig = null,
        Func<string, string>? dynamicFileNameFunction = null)
        => new(artifactData, new HttpRequestMessageArtifactResourceInfo(artifactTool, request, httpRequestConfig, key, contentType, updated, retrieved, version, checksum, dynamicFileNameFunction));

    /// <summary>
    /// Creates a <see cref="HttpRequestMessageArtifactResourceInfo"/> resource.
    /// </summary>
    /// <param name="artifactData">Source <see cref="ArtifactData"/> instance.</param>
    /// <param name="artifactTool">Artifact tool.</param>
    /// <param name="request">Request.</param>
    /// <param name="file">Filename.</param>
    /// <param name="path">Path.</param>
    /// <param name="contentType">MIME content type.</param>
    /// <param name="updated">Date this resource was updated.</param>
    /// <param name="retrieved">Date this resource was retrieved.</param>
    /// <param name="version">Version.</param>
    /// <param name="checksum">Checksum.</param>
    /// <param name="httpRequestConfig">Custom request configuration.</param>
    /// <param name="dynamicFileNameFunction">Function to use for transforming retrieved filename.</param>
    public static ArtifactDataResource HttpRequestMessage(this ArtifactData artifactData,
        HttpArtifactTool artifactTool,
        HttpRequestMessage request,
        string file,
        string path = "",
        string? contentType = "application/octet-stream",
        DateTimeOffset? updated = null,
        DateTimeOffset? retrieved = null,
        string? version = null,
        Checksum? checksum = null,
        HttpRequestConfig? httpRequestConfig = null,
        Func<string, string>? dynamicFileNameFunction = null)
        => new(artifactData, new HttpRequestMessageArtifactResourceInfo(artifactTool, request, httpRequestConfig, new ArtifactResourceKey(artifactData.Info.Key, file, path), contentType, updated, retrieved, version, checksum, dynamicFileNameFunction));

    /// <summary>
    /// Creates a <see cref="HttpRequestMessageArtifactResourceInfo"/> resource.
    /// </summary>
    /// <param name="artifactData">Source <see cref="ArtifactData"/> instance.</param>
    /// <param name="request">Request.</param>
    /// <param name="key">Resource key.</param>
    /// <param name="contentType">MIME content type.</param>
    /// <param name="updated">Date this resource was updated.</param>
    /// <param name="retrieved">Date this resource was retrieved.</param>
    /// <param name="version">Version.</param>
    /// <param name="checksum">Checksum.</param>
    /// <param name="httpRequestConfig">Custom request configuration.</param>
    /// <param name="dynamicFileNameFunction">Function to use for transforming retrieved filename.</param>
    public static ArtifactDataResource HttpRequestMessage(this ArtifactData artifactData,
        HttpRequestMessage request,
        ArtifactResourceKey key,
        string? contentType = "application/octet-stream",
        DateTimeOffset? updated = null,
        DateTimeOffset? retrieved = null,
        string? version = null,
        Checksum? checksum = null,
        HttpRequestConfig? httpRequestConfig = null,
        Func<string, string>? dynamicFileNameFunction = null)
        => artifactData.HttpRequestMessage(artifactData.GetArtifactTool<HttpArtifactTool>(), request, key, contentType, updated, retrieved, version, checksum, httpRequestConfig, dynamicFileNameFunction);

    /// <summary>
    /// Creates a <see cref="HttpRequestMessageArtifactResourceInfo"/> resource.
    /// </summary>
    /// <param name="artifactData">Source <see cref="ArtifactData"/> instance.</param>
    /// <param name="request">Request.</param>
    /// <param name="file">Filename.</param>
    /// <param name="path">Path.</param>
    /// <param name="contentType">MIME content type.</param>
    /// <param name="updated">Date this resource was updated.</param>
    /// <param name="retrieved">Date this resource was retrieved.</param>
    /// <param name="version">Version.</param>
    /// <param name="checksum">Checksum.</param>
    /// <param name="httpRequestConfig">Custom request configuration.</param>
    /// <param name="dynamicFileNameFunction">Function to use for transforming retrieved filename.</param>
    public static ArtifactDataResource HttpRequestMessage(this ArtifactData artifactData,
        HttpRequestMessage request,
        string file,
        string path = "",
        string? contentType = "application/octet-stream",
        DateTimeOffset? updated = null,
        DateTimeOffset? retrieved = null,
        string? version = null,
        Checksum? checksum = null,
        HttpRequestConfig? httpRequestConfig = null,
        Func<string, string>? dynamicFileNameFunction = null)
        => artifactData.HttpRequestMessage(artifactData.GetArtifactTool<HttpArtifactTool>(), request, file, path, contentType, updated, retrieved, version, checksum, httpRequestConfig, dynamicFileNameFunction);
}
