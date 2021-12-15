﻿namespace Art;

/// <summary>
/// Provides artifact information.
/// </summary>
/// <param name="Resource">Resource.</param>
/// <param name="Key">Resource key.</param>
/// <param name="ContentType">MIME content type.</param>
/// <param name="Updated">Updated date.</param>
/// <param name="Version">Version.</param>
public record StreamArtifactResourceInfo(Stream Resource, ArtifactResourceKey Key, string? ContentType = "application/octet-stream", DateTimeOffset? Updated = null, string? Version = null)
    : ArtifactResourceInfo(Key, ContentType, Updated, Version)
{
    /// <inheritdoc/>
    public override bool Exportable => true;

    /// <inheritdoc/>
    public override ValueTask<Stream> ExportStreamAsync(CancellationToken cancellationToken = default)
        => ValueTask.FromResult(Resource);
}
