using System.ComponentModel.DataAnnotations;

namespace Art.EF;

/// <summary>
/// Model type for <see cref="ArtifactInfo"/>.
/// </summary>
public class ArtifactInfoModel
{
    /// <summary>
    /// Tool id.
    /// </summary>
    [MaxLength(65536)]
    public virtual string Tool { get; set; } = null!;

    /// <summary>
    /// Group.
    /// </summary>
    [MaxLength(65536)]
    public virtual string Group { get; set; } = null!;

    /// <summary>
    /// Artifact id.
    /// </summary>
    [MaxLength(65536)]
    public virtual string Id { get; set; } = null!;

    /// <summary>
    /// Name.
    /// </summary>
    [MaxLength(65536)]
    public virtual string? Name { get; set; }

    /// <summary>
    /// Artifact creation date.
    /// </summary>
    public virtual DateTimeOffset? Date { get; set; }

    /// <summary>
    /// Artifact update date.
    /// </summary>
    public virtual DateTimeOffset? UpdateDate { get; set; }

    /// <summary>
    /// Artifact retrieval date.
    /// </summary>
    public virtual DateTimeOffset? RetrievalDate { get; set; }

    /// <summary>
    /// True if this is a full artifact.
    /// </summary>
    public virtual bool Full { get; set; }

    /// <summary>
    /// Resources.
    /// </summary>
    public virtual HashSet<ArtifactResourceInfoModel> Resources { get; set; } = null!;

    /// <summary>
    /// Converts model to info record.
    /// </summary>
    /// <param name="value">Model.</param>
    /// <returns>Record.</returns>
    public static implicit operator ArtifactInfo(ArtifactInfoModel value)
        => new(new ArtifactKey(value.Tool, value.Group, value.Id), value.Name, value.Date, value.UpdateDate, value.RetrievalDate, value.Full);

    /// <summary>
    /// Converts model to info record.
    /// </summary>
    /// <param name="value">Model.</param>
    /// <returns>Record.</returns>
    public static implicit operator ArtifactInfoModel(ArtifactInfo value)
        => new()
        {
            Tool = value.Key.Tool,
            Group = value.Key.Group,
            Id = value.Key.Id,
            Name = value.Name,
            Date = value.Date,
            UpdateDate = value.UpdateDate,
            RetrievalDate = value.RetrievalDate,
            Full = value.Full
        };
}
