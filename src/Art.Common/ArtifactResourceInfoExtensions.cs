namespace Art.Common;

/// <summary>
/// Extensions for <see cref="ArtifactResourceInfo"/>.
/// </summary>
public static class ArtifactResourceInfoExtensions
{
    /// <summary>
    /// Checks if non-identifying metadata (i.e. everything but key, updated date, version) is different.
    /// </summary>
    /// <param name="artifactResourceInfo">Self.</param>
    /// <param name="other">Resource to compare to.</param>
    /// <returns>True if any metadata is different or if other is null.</returns>
    public static bool IsMetadataDifferent(this ArtifactResourceInfo artifactResourceInfo, ArtifactResourceInfo? other)
    {
        ArgumentNullException.ThrowIfNull(artifactResourceInfo);
        if (other == null) return true;
        return artifactResourceInfo.ContentType != other.ContentType;
    }

    /// <summary>
    /// Gets informational path string.
    /// </summary>
    /// <returns>Info path string.</returns>
    public static string GetInfoPathString(this ArtifactResourceInfo artifactResourceInfo)
    {
        if (artifactResourceInfo.Key.Path.EndsWith('/'))
        {
            return $"{artifactResourceInfo.Key.Path}{artifactResourceInfo.Key.File}";
        }
        if (string.IsNullOrEmpty(artifactResourceInfo.Key.Path))
        {
            return artifactResourceInfo.Key.File;
        }
        return $"{artifactResourceInfo.Key.Path}/{artifactResourceInfo.Key.File}";
    }

    /// <summary>
    /// Gets informational string.
    /// </summary>
    /// <returns>Info string.</returns>
    public static string GetInfoString(this ArtifactResourceInfo artifactResourceInfo) => $"Path: {artifactResourceInfo.GetInfoPathString()}{(artifactResourceInfo.ContentType != null ? $"\nContent type: {artifactResourceInfo.ContentType}" : "")}{(artifactResourceInfo.Updated != null ? $"\nUpdated: {artifactResourceInfo.Updated}" : "")}{(artifactResourceInfo.Retrieved != null ? $"\nRetrieved: {artifactResourceInfo.Retrieved}" : "")}{(artifactResourceInfo.Version != null ? $"\nVersion: {artifactResourceInfo.Version}" : "")}{(artifactResourceInfo.Checksum != null ? $"\nChecksum: {artifactResourceInfo.Checksum.Id}:{Convert.ToHexString(artifactResourceInfo.Checksum.Value)}" : "")}";
}
