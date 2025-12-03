namespace Art.Common.Resources;

/// <summary>
/// Represents a resource wrapping another resource and disabling metadata.
/// </summary>
/// <param name="BaseArtifactResourceInfo">Base resource.</param>
public record NoMetadataArtifactResourceInfo(ArtifactResourceInfo BaseArtifactResourceInfo)
    : ArtifactResourceInfo(
        BaseArtifactResourceInfo.Key,
        BaseArtifactResourceInfo.ContentType,
        BaseArtifactResourceInfo.Updated,
        BaseArtifactResourceInfo.Retrieved,
        BaseArtifactResourceInfo.Version)
{
    /// <inheritdoc/>
    public override bool CanExportStream => BaseArtifactResourceInfo.CanExportStream;

    /// <inheritdoc />
    public override bool CanGetStream => BaseArtifactResourceInfo.CanGetStream;

    /// <inheritdoc/>
    public override ValueTask ExportStreamAsync(Stream targetStream, bool useLogger = true, CancellationToken cancellationToken = default)
        => BaseArtifactResourceInfo.ExportStreamAsync(targetStream, useLogger, cancellationToken);

    /// <inheritdoc/>
    public override ValueTask<Stream> GetStreamAsync(CancellationToken cancellationToken = default)
        => BaseArtifactResourceInfo.GetStreamAsync(cancellationToken);

    /// <inheritdoc/>
    public override bool UsesMetadata => false;

    /// <inheritdoc/>
    public override ValueTask<ArtifactResourceInfo> WithMetadataAsync(CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult<ArtifactResourceInfo>(this);
    }

    /// <inheritdoc />
    public override void AugmentOutputStreamOptions(ref OutputStreamOptions options)
        => BaseArtifactResourceInfo.AugmentOutputStreamOptions(ref options);
}
