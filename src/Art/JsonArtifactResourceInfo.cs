﻿using System.Text.Json;

namespace Art;

/// <summary>
/// Provides artifact information.
/// </summary>
/// <param name="Resource">Resource.</param>
/// <param name="SerializerOptions">Serializer options.</param>
/// <param name="Key">Resource key.</param>
/// <param name="ContentType">MIME content type.</param>
/// <param name="Updated">Updated date.</param>
/// <param name="Version">Version.</param>
/// <param name="Checksum">Checksum.</param>
public record JsonArtifactResourceInfo<T>(T Resource, JsonSerializerOptions? SerializerOptions, ArtifactResourceKey Key, string? ContentType = "application/json", DateTimeOffset? Updated = null, string? Version = null, Checksum? Checksum = null)
    : ArtifactResourceInfo(Key, ContentType, Updated, Version, Checksum)
{
    /// <inheritdoc/>
    public override bool Exportable => true;

    /// <inheritdoc/>
    public override async ValueTask<Stream> ExportStreamAsync(CancellationToken cancellationToken = default)
    {
        Stream stream = new MemoryStream();
        await JsonSerializer.SerializeAsync(stream, Resource, SerializerOptions, cancellationToken).ConfigureAwait(false);
        stream.Position = 0;
        return stream;
    }
}
