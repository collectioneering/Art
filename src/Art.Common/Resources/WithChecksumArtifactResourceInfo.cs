namespace Art.Common.Resources;

/// <summary>
/// Represents a resource with checksum.
/// </summary>
/// <param name="ChecksumValue">Checksum.</param>
/// <param name="BaseArtifactResourceInfo">Base resource.</param>
public record WithChecksumArtifactResourceInfo(Checksum? ChecksumValue, ArtifactResourceInfo BaseArtifactResourceInfo)
    : ArtifactResourceInfo(
        BaseArtifactResourceInfo.Key,
        BaseArtifactResourceInfo.ContentType,
        BaseArtifactResourceInfo.Updated,
        BaseArtifactResourceInfo.Retrieved,
        BaseArtifactResourceInfo.Version,
        ChecksumValue)
{
    /// <inheritdoc/>
    public override bool CanExportStream => BaseArtifactResourceInfo.CanExportStream;

    /// <inheritdoc />
    public override bool CanGetStream => BaseArtifactResourceInfo.CanGetStream;

    /// <inheritdoc/>
    public override ValueTask ExportStreamAsync(Stream targetStream, ArtifactResourceExportOptions? exportOptions = null, CancellationToken cancellationToken = default)
        => BaseArtifactResourceInfo.ExportStreamAsync(targetStream, exportOptions, cancellationToken);

    /// <inheritdoc/>
    public override ValueTask<Stream> GetStreamAsync(CancellationToken cancellationToken = default)
        => BaseArtifactResourceInfo.GetStreamAsync(cancellationToken);

    /// <inheritdoc/>
    public override bool UsesMetadata => BaseArtifactResourceInfo.UsesMetadata;

    /// <inheritdoc/>
    public override async ValueTask<ArtifactResourceInfo> WithMetadataAsync(CancellationToken cancellationToken = default)
    {
        ArtifactResourceInfo b = await BaseArtifactResourceInfo.WithMetadataAsync(cancellationToken).ConfigureAwait(false);
        return this with
        {
            BaseArtifactResourceInfo = b,
            ContentType = b.ContentType,
            Updated = b.Updated,
            Version = b.Version,
            Checksum = ChecksumValue
        };
    }

    /// <inheritdoc />
    public override void AugmentOutputStreamOptions(ref OutputStreamOptions options)
        => BaseArtifactResourceInfo.AugmentOutputStreamOptions(ref options);
}
