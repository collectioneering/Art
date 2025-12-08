namespace Art;

/// <summary>
/// Specifies options to use when exporting a resource.
/// </summary>
public record ArtifactResourceExportOptions
{
    /// <summary>
    /// Default options.
    /// </summary>
    public static readonly ArtifactResourceExportOptions Default = new() { IsConcurrent = false };

    /// <summary>
    /// If true, this export operation is known to be occurring in a concurrent manner with other operations.
    /// </summary>
    /// <remarks>
    /// This property should be used to conditionally disable features that need exclusive control of resources like
    /// single download progress bars.
    /// </remarks>
    public bool IsConcurrent { get; init; }
}
