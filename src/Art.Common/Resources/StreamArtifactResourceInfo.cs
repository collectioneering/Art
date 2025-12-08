using Art.Common.IO;

namespace Art.Common.Resources;

/// <summary>
/// Provides artifact information.
/// </summary>
/// <param name="Resource">Resource.</param>
/// <param name="Key">Resource key.</param>
/// <param name="ContentType">MIME content type.</param>
/// <param name="Updated">Date this resource was updated.</param>
/// <param name="Retrieved">Date this resource was retrieved.</param>
/// <param name="Version">Version.</param>
/// <param name="Checksum">Checksum.</param>
public record StreamArtifactResourceInfo(
    Stream Resource,
    ArtifactResourceKey Key,
    string? ContentType = "application/octet-stream",
    DateTimeOffset? Updated = null,
    DateTimeOffset? Retrieved = null,
    string? Version = null,
    Checksum? Checksum = null)
    : ArtifactResourceInfo(Key, ContentType, Updated, Retrieved, Version, Checksum)
{
    /// <inheritdoc/>
    public override bool CanExportStream => Resource.CanRead;

    /// <inheritdoc />
    public override bool CanGetStream
    {
        get
        {
            // Require CanSeek so repeatability is always possible
            return Resource.CanRead && Resource.CanSeek;
        }
    }

    /// <inheritdoc/>
    public override async ValueTask ExportStreamAsync(Stream targetStream, ArtifactResourceExportOptions? exportOptions = null, CancellationToken cancellationToken = default)
    {
        long? position = Resource.CanSeek ? Resource.Position : null;
        try
        {
            await Resource.CopyToAsync(targetStream, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            if (position is { } p) Resource.Position = p;
        }
    }

    /// <inheritdoc />
    public override ValueTask<Stream> GetStreamAsync(CancellationToken cancellationToken = default)
    {
        Resource.Position = 0;
        // Critically, record behaviour should be reusability, so wrap it in a non-disposing wrapper
        // ??? should NonDisposingStream also have isolation behaviour like LimitedStream?
        return new ValueTask<Stream>(new NonDisposingStream(Resource));
    }

    /// <inheritdoc />
    public override void AugmentOutputStreamOptions(ref OutputStreamOptions options)
    {
        if (Resource.CanSeek) options = options with { PreallocationSize = Resource.Length };
    }
}

public partial class ArtifactDataExtensions
{
    /// <summary>
    /// Creates a <see cref="StreamArtifactResourceInfo"/> resource.
    /// </summary>
    /// <param name="artifactData">Source <see cref="ArtifactData"/> instance.</param>
    /// <param name="resource">Resource.</param>
    /// <param name="key">Resource key.</param>
    /// <param name="contentType">MIME content type.</param>
    /// <param name="updated">Date this resource was updated.</param>
    /// <param name="retrieved">Date this resource was retrieved.</param>
    /// <param name="version">Version.</param>
    /// <param name="checksum">Checksum.</param>
    public static ArtifactDataResource Stream(
        this ArtifactData artifactData,
        Stream resource,
        ArtifactResourceKey key,
        string? contentType = "application/octet-stream",
        DateTimeOffset? updated = null,
        DateTimeOffset? retrieved = null,
        string? version = null,
        Checksum? checksum = null)
        => new(artifactData, new StreamArtifactResourceInfo(resource, key, contentType, updated, retrieved, version, checksum));

    /// <summary>
    /// Creates a <see cref="StreamArtifactResourceInfo"/> resource.
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
    public static ArtifactDataResource Stream(
        this ArtifactData artifactData,
        Stream resource,
        string file,
        string path = "",
        string? contentType = "application/octet-stream",
        DateTimeOffset? updated = null,
        DateTimeOffset? retrieved = null,
        string? version = null,
        Checksum? checksum = null)
        => new(artifactData, new StreamArtifactResourceInfo(resource, new ArtifactResourceKey(artifactData.Info.Key, file, path), contentType, updated, retrieved, version, checksum));
}
