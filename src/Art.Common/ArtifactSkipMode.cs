namespace Art.Common;

/// <summary>
/// Represents artifact skip mode.
/// </summary>
public enum ArtifactSkipMode
{
    /// <summary>
    /// Never skip.
    /// </summary>
    None,

    /// <summary>
    /// Skip all artifacts starting from first known artifact.
    /// </summary>
    FastExit,

    /// <summary>
    /// Skip only known artifacts.
    /// </summary>
    Known,

    /// <summary>
    /// Skip only known artifacts which are <see cref="ArtifactInfo.Full"/>.
    /// </summary>
    KnownFull
}
