namespace Art.Common.Resources;

/// <summary>
/// Represents a resource with content type.
/// </summary>
/// <param name="ContentTypeValue">Content type.</param>
/// <param name="BaseArtifactResourceInfo">Base resource.</param>
public record WithContentTypeArtifactResourceInfo(string? ContentTypeValue, ArtifactResourceInfo BaseArtifactResourceInfo)
    : ArtifactResourceInfo(
        BaseArtifactResourceInfo.Key,
        ContentTypeValue,
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
    public override bool UsesMetadata => BaseArtifactResourceInfo.UsesMetadata;

    /// <inheritdoc/>
    public override async ValueTask<ArtifactResourceInfo> WithMetadataAsync(CancellationToken cancellationToken = default)
    {
        ArtifactResourceInfo b = await BaseArtifactResourceInfo.WithMetadataAsync(cancellationToken).ConfigureAwait(false);
        return this with
        {
            BaseArtifactResourceInfo = b,
            ContentType = ContentTypeValue,
            Updated = b.Updated,
            Version = b.Version,
            Checksum = b.Checksum
        };
    }

    /// <inheritdoc />
    public override void AugmentOutputStreamOptions(ref OutputStreamOptions options)
        => BaseArtifactResourceInfo.AugmentOutputStreamOptions(ref options);
}
