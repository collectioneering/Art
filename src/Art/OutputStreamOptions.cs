namespace Art;

/// <summary>
/// Represents options for creating artifact output stream.
/// </summary>
public record OutputStreamOptions
{
    /// <summary>
    /// Default options.
    /// </summary>
    public static readonly OutputStreamOptions Default = new() { PreallocationSize = 0, PreferTemporaryLocation = true };

    /// <summary>
    /// Stream preallocation size.
    /// </summary>
    /// <remarks>
    /// A value of 0 is ignored. Negative values are invalid.
    /// </remarks>
    public long PreallocationSize { get; init; }

    /// <summary>
    /// If true, prefer using temporary location when possible.
    /// </summary>
    /// <remarks>
    /// This option enables behaviour such that, if possible for the <see cref="IArtifactDataManager"/>
    /// and <see cref="ICommittable"/>, content is written to a temporary location (e.g. a file
    /// with a non-colliding name) and is moved to the final location on commit.
    /// </remarks>
    public bool PreferTemporaryLocation { get; init; }
}
