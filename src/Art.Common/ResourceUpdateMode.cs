namespace Art.Common;

/// <summary>
/// Modes for determining how resources are updated.
/// </summary>
public enum ResourceUpdateMode
{
    /// <summary>
    /// Update resource if and only if an artifact has been updated and a new version is detected.
    /// </summary>
    ArtifactSoft,
    /// <summary>
    /// Update resource if and only if an artifact has been updated.
    /// </summary>
    ArtifactHard,
    /// <summary>
    /// Update resource if new version is detected.
    /// </summary>
    Soft,
    /// <summary>
    /// Always retrieve resource.
    /// </summary>
    Hard
}
