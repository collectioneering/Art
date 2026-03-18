namespace Art.Common.Resources;

/// <summary>
/// Represents a resource based on a file on disk via <see cref="FileInfo"/>.
/// </summary>
public record FileArtifactResourceInfo(
    FileInfo Resource,
    ArtifactResourceKey Key,
    string? ContentType = "application/octet-stream",
    DateTimeOffset? Updated = null,
    DateTimeOffset? Retrieved = null,
    string? Version = null,
    Checksum? Checksum = null)
    : ArtifactResourceInfo(Key, ContentType, Updated, Retrieved, Version, Checksum)
{
    /// <inheritdoc />
    public override bool CanExportStream => Resource.Exists;

    /// <inheritdoc />
    public override bool CanGetStream => Resource.Exists;

    /// <inheritdoc />
    public override async ValueTask ExportStreamAsync(Stream targetStream, ArtifactResourceExportOptions? exportOptions = null, CancellationToken cancellationToken = default)
    {
        await using var fileStream = Resource.OpenRead();
        await fileStream.CopyToAsync(targetStream, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override ValueTask<Stream> GetStreamAsync(CancellationToken cancellationToken = default)
    {
        return new ValueTask<Stream>(Resource.OpenRead());
    }

    /// <inheritdoc />
    public override void AugmentOutputStreamOptions(ref OutputStreamOptions options)
    {
        if (Resource.Exists)
        {
            options = options with { PreallocationSize = Resource.Length };
        }
    }

    /// <inheritdoc />
    public override ValueTask<ArtifactResourceInfo> WithMetadataAsync(CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult<ArtifactResourceInfo>(Resource.Exists ? this with { Updated = Resource.LastWriteTime } : this);
    }
}

public partial class ArtifactDataExtensions
{
    /// <summary>
    /// Creates a <see cref="FileArtifactResourceInfo"/> resource.
    /// </summary>
    /// <param name="artifactData">Source <see cref="ArtifactData"/> instance.</param>
    /// <param name="resource">Resource.</param>
    /// <param name="key">Resource key.</param>
    /// <param name="contentType">MIME content type.</param>
    /// <param name="updated">Date this resource was updated.</param>
    /// <param name="retrieved">Date this resource was retrieved.</param>
    /// <param name="version">Version.</param>
    /// <param name="checksum">Checksum.</param>
    public static ArtifactDataResource File(
        this ArtifactData artifactData,
        FileInfo resource,
        ArtifactResourceKey key,
        string? contentType = "",
        DateTimeOffset? updated = null,
        DateTimeOffset? retrieved = null,
        string? version = null,
        Checksum? checksum = null)
        => new(artifactData, new FileArtifactResourceInfo(resource, key, contentType, updated, retrieved, version, checksum));

    /// <summary>
    /// Creates a <see cref="FileArtifactResourceInfo"/> resource.
    /// </summary>
    /// <param name="artifactData">Source <see cref="ArtifactData"/> instance.</param>
    /// <param name="resource">Resource.</param>
    /// <param name="file">Filename.</param>
    /// <param name="path">Path.</param>
    /// <param name="contentType">MIME content type.</param>
    /// <param name="updated">Date this resource was updated.</param>
    /// <param name="retrieved">Date this resource was retrieved.</param>
    /// <param name="version">Version.</param>
    /// <param name="checksum">Checksum.</param>
    public static ArtifactDataResource File(
        this ArtifactData artifactData,
        FileInfo resource,
        string file,
        string path = "",
        string? contentType = "application/json",
        DateTimeOffset? updated = null,
        DateTimeOffset? retrieved = null,
        string? version = null,
        Checksum? checksum = null)
        => new(artifactData, new FileArtifactResourceInfo(resource, new ArtifactResourceKey(artifactData.Info.Key, file, path), contentType, updated, retrieved, version, checksum));
}
