namespace Art.Common.Resources;

/// <summary>
/// Represents a resource with version.
/// </summary>
/// <param name="VersionValue">Version.</param>
/// <param name="BaseArtifactResourceInfo">Base resource.</param>
public record WithVersionArtifactResourceInfo(string? VersionValue, ArtifactResourceInfo BaseArtifactResourceInfo)
    : ArtifactResourceInfo(
        BaseArtifactResourceInfo.Key,
        BaseArtifactResourceInfo.ContentType,
        BaseArtifactResourceInfo.Updated,
        BaseArtifactResourceInfo.Retrieved,
        VersionValue)
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
            Version = VersionValue,
            Checksum = b.Checksum
        };
    }

    /// <inheritdoc />
    public override void AugmentOutputStreamOptions(ref OutputStreamOptions options)
        => BaseArtifactResourceInfo.AugmentOutputStreamOptions(ref options);
}
