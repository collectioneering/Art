﻿using System.Text;

namespace Art.Common.Resources;

/// <summary>
/// Provides artifact information.
/// </summary>
/// <param name="Resource">Resource.</param>
/// <param name="Key">Resource key.</param>
/// <param name="ContentType">MIME content type.</param>
/// <param name="Updated">Updated date.</param>
/// <param name="Version">Version.</param>
/// <param name="Checksum">Checksum.</param>
public record StringArtifactResourceInfo(string Resource, ArtifactResourceKey Key, string? ContentType = "text/plain", DateTimeOffset? Updated = null, string? Version = null, Checksum? Checksum = null)
    : ArtifactResourceInfo(Key, ContentType, Updated, Version, Checksum)
{
    /// <inheritdoc/>
    public override bool CanExportStream => true;

    /// <inheritdoc />
    public override bool CanGetStream => true;

    /// <inheritdoc/>
    public override async ValueTask ExportStreamAsync(Stream targetStream, CancellationToken cancellationToken = default)
    {
        await using StreamWriter sw = new(targetStream, Encoding.UTF8, leaveOpen: true);
        await sw.WriteAsync(Resource).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override async ValueTask<Stream> GetStreamAsync(CancellationToken cancellationToken = default)
    {
        // streaming serialization is not supported, just https://www.youtube.com/watch?v=VQqO20pVhpk it
        var ms = new MemoryStream();
        await ExportStreamAsync(ms, cancellationToken).ConfigureAwait(false);
        ms.Position = 0;
        return ms;
    }
}

public partial class ArtifactDataExtensions
{
    /// <summary>
    /// Creates a <see cref="StringArtifactResourceInfo"/> resource.
    /// </summary>
    /// <param name="artifactData">Source <see cref="ArtifactData"/> instance.</param>
    /// <param name="resource">Resource.</param>
    /// <param name="key">Resource key.</param>
    /// <param name="contentType">MIME content type.</param>
    /// <param name="updated">Updated date.</param>
    /// <param name="version">Version.</param>
    /// <param name="checksum">Checksum.</param>
    public static ArtifactDataResource String(this ArtifactData artifactData, string resource, ArtifactResourceKey key, string? contentType = "text/plain", DateTimeOffset? updated = null, string? version = null, Checksum? checksum = null)
        => new(artifactData, new StringArtifactResourceInfo(resource, key, contentType, updated, version, checksum));

    /// <summary>
    /// Creates a <see cref="StringArtifactResourceInfo"/> resource.
    /// </summary>
    /// <param name="artifactData">Source <see cref="ArtifactData"/> instance.</param>
    /// <param name="resource">Resource.</param>
    /// <param name="file">Filename.</param>
    /// <param name="path">Path.</param>
    /// <param name="contentType">MIME content type.</param>
    /// <param name="updated">Updated date.</param>
    /// <param name="version">Version.</param>
    /// <param name="checksum">Checksum.</param>
    public static ArtifactDataResource String(this ArtifactData artifactData, string resource, string file, string path = "", string? contentType = "text/plain", DateTimeOffset? updated = null, string? version = null, Checksum? checksum = null)
        => new(artifactData, new StringArtifactResourceInfo(resource, new ArtifactResourceKey(artifactData.Info.Key, file, path), contentType, updated, version, checksum));
}
