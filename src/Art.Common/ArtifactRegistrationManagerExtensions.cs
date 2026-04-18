namespace Art.Common;

/// <summary>
/// Extensions for <see cref="IArtifactRegistrationManager"/>.
/// </summary>
public static class ArtifactRegistrationManagerExtensions
{
    /// <summary>
    /// Lists all artifacts for the specified tool and group, where each is optional.
    /// </summary>
    /// <param name="artifactRegistrationManager">Manager.</param>
    /// <param name="tool">Tool (optional).</param>
    /// <param name="group">Group (optional).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning artifacts.</returns>
    public static async Task<IEnumerable<ArtifactInfo>> ListArtifactsOptionalsAsync(this IArtifactRegistrationManager artifactRegistrationManager, string? tool, string? group, CancellationToken cancellationToken = default)
    {
        IEnumerable<ArtifactInfo> enumerable;
        if (tool != null)
        {
            enumerable = group != null ? await artifactRegistrationManager.ListArtifactsAsync(tool, group, cancellationToken).ConfigureAwait(false) : await artifactRegistrationManager.ListArtifactsAsync(tool, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            enumerable = group != null ? (await artifactRegistrationManager.ListArtifactsAsync(cancellationToken).ConfigureAwait(false)).Where(v => v.Key.Group == group) : await artifactRegistrationManager.ListArtifactsAsync(cancellationToken).ConfigureAwait(false);
        }
        return enumerable;
    }
}
