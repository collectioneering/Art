namespace Art.Common.Proxies;

/// <summary>
/// Defines different orderings for artifacts.
/// </summary>
public enum ArtifactSortOrder
{
    /// <summary>
    /// Default sort order, sorts artifacts as-is.
    /// </summary>
    Default,

    /// <summary>
    /// Sorts artifacts by <see cref="ArtifactInfo.UpdateDate"/>, with newer values taking precedence.
    /// </summary>
    /// <remarks>
    /// Entries with a definite <see cref="ArtifactInfo.UpdateDate"/> are ordered first,
    /// followed by descending entries with only <see cref="ArtifactInfo.Date"/>, and finally
    /// entries with neither value, sorted by descending group then key.
    /// </remarks>
    UpdatedNewestFirst,

    /// <summary>
    /// Sorts artifacts by <see cref="ArtifactInfo.UpdateDate"/>, with older values taking precedence.
    /// </summary>
    /// <remarks>
    /// Entries with a definite <see cref="ArtifactInfo.UpdateDate"/> are ordered first,
    /// followed by ascending entries with only <see cref="ArtifactInfo.Date"/>, and finally
    /// entries with neither value, sorted by ascending group then key.
    /// </remarks>
    UpdatedOldestFirst,
}
