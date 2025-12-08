using System.Text.Json.Serialization;

namespace Art;

/// <summary>
/// Provides artifact information.
/// </summary>
/// <param name="Key">Resource key.</param>
/// <param name="ContentType">MIME content type.</param>
/// <param name="Updated">Date this resource was updated.</param>
/// <param name="Retrieved">Date this resource was retrieved.</param>
/// <param name="Version">Version.</param>
/// <param name="Checksum">Checksum.</param>
public record ArtifactResourceInfo(ArtifactResourceKey Key, string? ContentType = "application/octet-stream", DateTimeOffset? Updated = null, DateTimeOffset? Retrieved = null, string? Version = null, Checksum? Checksum = null)
{
    /// <summary>
    /// If true, it is possible to export the content referred to by this reference via <see cref="ExportStreamAsync"/>.
    /// </summary>
    [JsonIgnore]
    public virtual bool CanExportStream => false;

    /// <summary>
    /// If true, it is possible to directly get a stream from this reference via <see cref="GetStreamAsync"/>.
    /// </summary>
    [JsonIgnore]
    public virtual bool CanGetStream => false;

    /// <summary>
    /// If true, this resource can be queried for additional metadata.
    /// </summary>
    [JsonIgnore]
    public virtual bool UsesMetadata => false;

    /// <summary>
    /// Exports a resource.
    /// </summary>
    /// <param name="targetStream">Stream to write resource contents to.</param>
    /// <param name="exportOptions">Options to use for export operation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="NotSupportedException">Thrown if this instance cannot be exported.</exception>
    public virtual ValueTask ExportStreamAsync(Stream targetStream, ArtifactResourceExportOptions? exportOptions = null, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"Exporting a stream from this instance of {nameof(ArtifactResourceInfo)} is not supported");
    }

    /// <summary>
    /// Gets a stream for this resource.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning resource stream.</returns>
    public virtual ValueTask<Stream> GetStreamAsync(CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException($"Getting a stream from this instance of {nameof(ArtifactResourceInfo)} is not supported");
    }

    /// <summary>
    /// Gets this resource with associated metadata, if available.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Resource with metadata if available.</returns>
    public virtual ValueTask<ArtifactResourceInfo> WithMetadataAsync(CancellationToken cancellationToken = default)
        => ValueTask.FromResult(this);

    /// <summary>
    /// Adjusts output stream options to suit outputting this resource.
    /// </summary>
    /// <param name="options">Output options.</param>
    public virtual void AugmentOutputStreamOptions(ref OutputStreamOptions options)
    {
    }
}
