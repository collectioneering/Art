﻿using System.Text;

namespace Art;

/// <summary>
/// Provides artifact information.
/// </summary>
/// <param name="Resource">Resource.</param>
/// <param name="Key">Resource key.</param>
/// <param name="Version">Version.</param>
public record StringArtifactResourceInfo(string Resource, ArtifactResourceKey Key, string? Version = null)
    : ArtifactResourceInfo(Key, Version)
{
    /// <inheritdoc/>
    public override bool Exportable => true;

    /// <inheritdoc/>
    public override async ValueTask ExportAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        using var sw = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);
        await sw.WriteAsync(Resource).ConfigureAwait(false);
    }
}
