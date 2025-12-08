namespace Art.M3U.Resources;

/// <summary>
/// Represents an HLS stream.
/// </summary>
/// <param name="Saver">Stream context.</param>
/// <param name="Key">Resource key.</param>
/// <param name="ContentType">MIME content type.</param>
/// <param name="Updated">Date this resource was updated.</param>
/// <param name="Retrieved">Date this resource was retrieved.</param>
/// <param name="Version">Version.</param>
/// <param name="Checksum">Checksum.</param>
public record M3UDownloaderContextSaverArtifactResourceInfo(
    M3UDownloaderContextStreamOutputSaver Saver,
    ArtifactResourceKey Key,
    string? ContentType = "application/octet-stream",
    DateTimeOffset? Updated = null,
    DateTimeOffset? Retrieved = null,
    string? Version = null, Checksum? Checksum = null) : ArtifactResourceInfo(Key, ContentType, Updated, Retrieved, Version, Checksum)
{
    /// <inheritdoc />
    public override bool CanExportStream => true;

    /// <inheritdoc />
    public override async ValueTask ExportStreamAsync(Stream targetStream, ArtifactResourceExportOptions? exportOptions = null, CancellationToken cancellationToken = default)
    {
        await Saver.ExportAsync(targetStream, cancellationToken).ConfigureAwait(false);
    }
}
