﻿namespace Art;

/// <summary>
/// Provides artifact information.
/// </summary>
/// <param name="ArtifactTool">Artifact tool.</param>
/// <param name="Request">Request.</param>
/// <param name="Key">Resource key.</param>
/// <param name="ContentType">MIME content type.</param>
/// <param name="Updated">Updated date.</param>
/// <param name="Version">Version.</param>
public record HttpRequestMessageArtifactResourceInfo(HttpArtifactTool ArtifactTool, HttpRequestMessage Request, ArtifactResourceKey Key, string? ContentType = "application/octet-stream", DateTimeOffset? Updated = null, string? Version = null)
    : QueryBaseArtifactResourceInfo(Key, ContentType, Updated, Version)
{
    /// <inheritdoc/>
    public override bool Exportable => true;

    /// <inheritdoc/>
    public override async ValueTask<Stream> ExportStreamAsync(CancellationToken cancellationToken = default)
    {
        Stream stream = new MemoryStream();
        await ArtifactTool.DownloadResourceAsync(Request, stream, cancellationToken).ConfigureAwait(false);
        stream.Position = 0;
        return stream;
    }
}
