﻿using Art.Common;

namespace Art.Http.Resources;

/// <summary>
/// Provides artifact information.
/// </summary>
/// <param name="ArtifactTool">Artifact tool.</param>
/// <param name="Request">Request.</param>
/// <param name="Key">Resource key.</param>
/// <param name="ContentType">MIME content type.</param>
/// <param name="Updated">Updated date.</param>
/// <param name="Version">Version.</param>
/// <param name="Checksum">Checksum.</param>
/// <param name="HttpRequestConfig">Custom request configuration.</param>
public record HttpRequestMessageArtifactResourceInfo(
        HttpArtifactTool ArtifactTool,
        HttpRequestMessage Request,
        HttpRequestConfig? HttpRequestConfig,
        ArtifactResourceKey Key,
        string? ContentType = "application/octet-stream",
        DateTimeOffset? Updated = null,
        string? Version = null,
        Checksum? Checksum = null)
    : QueryBaseArtifactResourceInfo(Key, ContentType, Updated, Version, Checksum)
{
    /// <inheritdoc/>
    public override bool CanExportStream => true;

    /// <inheritdoc />
    public override bool CanGetStream => true;

    /// <inheritdoc/>
    /// <exception cref="TaskCanceledException">Thrown with <see cref="TimeoutException"/> <see cref="Exception.InnerException"/> for a timeout.</exception>
    /// <exception cref="HttpRequestException">Thrown for issues with request excluding non-success server responses.</exception>
    /// <exception cref="ArtHttpResponseMessageException">Thrown on HTTP response indicating non-successful response.</exception>
    public override async ValueTask ExportStreamAsync(Stream targetStream, CancellationToken cancellationToken = default)
    {
        await ArtifactTool.DownloadResourceAsync(Request, targetStream, HttpRequestConfig, cancellationToken).ConfigureAwait(false);
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
    /// <param name="updated">Updated date.</param>
    /// <param name="version">Version.</param>
    /// <param name="checksum">Checksum.</param>
    /// <param name="httpRequestConfig">Custom request configuration.</param>
    public static ArtifactDataResource HttpRequestMessage(this ArtifactData artifactData,
        HttpArtifactTool artifactTool,
        HttpRequestMessage request,
        ArtifactResourceKey key,
        string? contentType = "application/octet-stream",
        DateTimeOffset? updated = null,
        string? version = null,
        Checksum? checksum = null,
        HttpRequestConfig? httpRequestConfig = null)
        => new(artifactData, new HttpRequestMessageArtifactResourceInfo(artifactTool, request, httpRequestConfig, key, contentType, updated, version, checksum));

    /// <summary>
    /// Creates a <see cref="HttpRequestMessageArtifactResourceInfo"/> resource.
    /// </summary>
    /// <param name="artifactData">Source <see cref="ArtifactData"/> instance.</param>
    /// <param name="artifactTool">Artifact tool.</param>
    /// <param name="request">Request.</param>
    /// <param name="file">Filename.</param>
    /// <param name="path">Path.</param>
    /// <param name="contentType">MIME content type.</param>
    /// <param name="updated">Updated date.</param>
    /// <param name="version">Version.</param>
    /// <param name="checksum">Checksum.</param>
    /// <param name="httpRequestConfig">Custom request configuration.</param>
    public static ArtifactDataResource HttpRequestMessage(this ArtifactData artifactData,
        HttpArtifactTool artifactTool,
        HttpRequestMessage request,
        string file,
        string path = "",
        string? contentType = "application/octet-stream",
        DateTimeOffset? updated = null,
        string? version = null,
        Checksum? checksum = null,
        HttpRequestConfig? httpRequestConfig = null)
        => new(artifactData, new HttpRequestMessageArtifactResourceInfo(artifactTool, request, httpRequestConfig, new ArtifactResourceKey(artifactData.Info.Key, file, path), contentType, updated, version, checksum));

    /// <summary>
    /// Creates a <see cref="HttpRequestMessageArtifactResourceInfo"/> resource.
    /// </summary>
    /// <param name="artifactData">Source <see cref="ArtifactData"/> instance.</param>
    /// <param name="request">Request.</param>
    /// <param name="key">Resource key.</param>
    /// <param name="contentType">MIME content type.</param>
    /// <param name="updated">Updated date.</param>
    /// <param name="version">Version.</param>
    /// <param name="checksum">Checksum.</param>
    /// <param name="httpRequestConfig">Custom request configuration.</param>
    public static ArtifactDataResource HttpRequestMessage(this ArtifactData artifactData,
        HttpRequestMessage request,
        ArtifactResourceKey key,
        string? contentType = "application/octet-stream",
        DateTimeOffset? updated = null,
        string? version = null,
        Checksum? checksum = null,
        HttpRequestConfig? httpRequestConfig = null)
        => artifactData.HttpRequestMessage(artifactData.GetArtifactTool<HttpArtifactTool>(), request, key, contentType, updated, version, checksum);

    /// <summary>
    /// Creates a <see cref="HttpRequestMessageArtifactResourceInfo"/> resource.
    /// </summary>
    /// <param name="artifactData">Source <see cref="ArtifactData"/> instance.</param>
    /// <param name="request">Request.</param>
    /// <param name="file">Filename.</param>
    /// <param name="path">Path.</param>
    /// <param name="contentType">MIME content type.</param>
    /// <param name="updated">Updated date.</param>
    /// <param name="version">Version.</param>
    /// <param name="checksum">Checksum.</param>
    /// <param name="httpRequestConfig">Custom request configuration.</param>
    public static ArtifactDataResource HttpRequestMessage(this ArtifactData artifactData,
        HttpRequestMessage request,
        string file,
        string path = "",
        string? contentType = "application/octet-stream",
        DateTimeOffset? updated = null,
        string? version = null,
        Checksum? checksum = null,
        HttpRequestConfig? httpRequestConfig = null)
        => artifactData.HttpRequestMessage(artifactData.GetArtifactTool<HttpArtifactTool>(), request, file, path, contentType, updated, version, checksum, httpRequestConfig);
}
