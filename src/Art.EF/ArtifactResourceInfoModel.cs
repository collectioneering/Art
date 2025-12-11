using System.ComponentModel.DataAnnotations;

namespace Art.EF;

/// <summary>
/// Model type for <see cref="ArtifactResourceInfo"/>.
/// </summary>
public class ArtifactResourceInfoModel
{
    /// <summary>
    /// Tool id.
    /// </summary>
    [MaxLength(65536)]
    public virtual string ArtifactTool { get; set; } = null!;

    /// <summary>
    /// Group.
    /// </summary>
    [MaxLength(65536)]
    public virtual string ArtifactGroup { get; set; } = null!;

    /// <summary>
    /// Artifact id.
    /// </summary>
    [MaxLength(65536)]
    public virtual string ArtifactId { get; set; } = null!;

    /// <summary>
    /// Artifact.
    /// </summary>
    public virtual ArtifactInfoModel Artifact { get; set; } = null!;

    /// <summary>
    /// Filename.
    /// </summary>
    [MaxLength(65536)]
    public virtual string File { get; set; } = null!;

    /// <summary>
    /// Path.
    /// </summary>
    [MaxLength(65536)]
    public virtual string Path { get; set; } = null!;

    /// <summary>
    /// Content type.
    /// </summary>
    [MaxLength(65536)]
    public virtual string? ContentType { get; set; }

    /// <summary>
    /// Date this resource was updated.
    /// </summary>
    public virtual DateTimeOffset? Updated { get; set; }

    /// <summary>
    /// Date this resource was retrieved.
    /// </summary>
    public virtual DateTimeOffset? Retrieved { get; set; }

    /// <summary>
    /// Version.
    /// </summary>
    [MaxLength(65536)]
    public virtual string? Version { get; set; }

    /// <summary>
    /// Checksum algorithm ID.
    /// </summary>
    [MaxLength(65536)]
    public virtual string? ChecksumId { get; set; }

    /// <summary>
    /// Checksum value.
    /// </summary>
    public virtual byte[]? ChecksumValue { get; set; }

    /// <summary>
    /// Converts model to info record.
    /// </summary>
    /// <param name="value">Model.</param>
    /// <returns>Record.</returns>
    public static implicit operator ArtifactResourceInfo(ArtifactResourceInfoModel value)
        => new(new ArtifactResourceKey(new ArtifactKey(value.ArtifactTool, value.ArtifactGroup, value.ArtifactId), value.File, value.Path), value.ContentType, value.Updated, value.Retrieved, value.Version,
            value.ChecksumId != null && value.ChecksumValue != null ? new Checksum(value.ChecksumId, value.ChecksumValue) : null);

    /// <summary>
    /// Converts info record to model.
    /// </summary>
    /// <param name="value">Record.</param>
    /// <returns>Model.</returns>
    public static implicit operator ArtifactResourceInfoModel(ArtifactResourceInfo value) =>
        new()
        {
            ArtifactTool = value.Key.Artifact.Tool,
            ArtifactGroup = value.Key.Artifact.Group,
            ArtifactId = value.Key.Artifact.Id,
            File = value.Key.File,
            Path = value.Key.Path,
            ContentType = value.ContentType,
            Updated = value.Updated,
            Retrieved = value.Retrieved,
            Version = value.Version,
            ChecksumId = value.Checksum?.Id,
            ChecksumValue = value.Checksum?.Value
        };
}
