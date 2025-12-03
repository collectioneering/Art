namespace Art.M3U;

/// <summary>
/// Alternate stream info.
/// </summary>
public class AlternateStreamInfo
{
    /// <summary>
    /// Path.
    /// </summary>
    public string Path { get; internal set; } = "";

    /// <summary>
    /// True if default.
    /// </summary>
    public bool Default { get; internal set; }

    /// <summary>
    /// True if autoselect
    /// </summary>
    public bool Autoselect { get; internal set; }

    /// <summary>
    /// Language.
    /// </summary>
    public string? Language { get; internal set; }

    /// <summary>
    /// Group ID.
    /// </summary>
    public string? GroupId { get; internal set; }

    /// <summary>
    /// Type.
    /// </summary>
    public string? Type { get; internal set; }

    /// <summary>
    /// Name.
    /// </summary>
    public string? Name { get; internal set; }
}
