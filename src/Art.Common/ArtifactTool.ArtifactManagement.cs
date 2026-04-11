namespace Art.Common;

public partial class ArtifactTool
{
    #region Artifact management

    /// <summary>
    /// Resolves applicable group.
    /// </summary>
    /// <param name="customGroup">Optional custom group to apply.</param>
    /// <returns>Group.</returns>
    public string ResolveGroup(string? customGroup = null)
    {
        // Group precedence: group specified in profile (treated as override), custom group specified locally, tool's fallback
        return Profile.Group ?? customGroup ?? GroupFallback;
    }

    /// <summary>
    /// Creates a new instance of <see cref="ArtifactData"/>.
    /// </summary>
    /// <param name="id">Artifact ID.</param>
    /// <param name="name">Name.</param>
    /// <param name="date">Artifact creation date.</param>
    /// <param name="updateDate">Artifact update date.</param>
    /// <param name="retrievalDate">Artifact retrieval date.</param>
    /// <param name="full">True if this is a full artifact.</param>
    /// <param name="group">Custom group.</param>
    public ArtifactData CreateData(
        string id,
        string? name = null,
        DateTimeOffset? date = null,
        DateTimeOffset? updateDate = null,
        DateTimeOffset? retrievalDate = null,
        bool full = true,
        string? group = null)
    {
        if (retrievalDate == null && Config.GetArtifactRetrievalTimestamps)
        {
            retrievalDate = TimeProvider.GetUtcNow();
        }
        return new ArtifactData(this, ResolveArtifactKey(id, group), name, date, updateDate, retrievalDate, full);
    }

    /// <summary>
    /// Resolves the key that would be produced for an ID and group.
    /// </summary>
    /// <param name="id">Artifact ID.</param>
    /// <param name="group">Custom group.</param>
    /// <returns>Artifact key.</returns>
    public virtual ArtifactKey ResolveArtifactKey(string id, string? group = null)
    {
        return new ArtifactKey(Profile.Tool, ResolveGroup(group), id);
    }

    /// <summary>
    /// Registers artifact as known.
    /// </summary>
    /// <param name="artifactInfo">Artifact to register.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    public async Task AddArtifactAsync(ArtifactInfo artifactInfo, CancellationToken cancellationToken = default)
        => await RegistrationManager.AddArtifactAsync(artifactInfo, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Registers artifact resource as known.
    /// </summary>
    /// <param name="artifactResourceInfo">Artifact resource to register.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    public async Task AddResourceAsync(ArtifactResourceInfo artifactResourceInfo, CancellationToken cancellationToken = default)
        => await RegistrationManager.AddResourceAsync(artifactResourceInfo, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Attempts to get info for the artifact with the specified ID.
    /// </summary>
    /// <param name="id">Artifact ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning retrieved artifact, if it exists.</returns>
    public async Task<ArtifactInfo?> TryGetArtifactAsync(string id, CancellationToken cancellationToken = default)
        => await RegistrationManager.TryGetArtifactAsync(new ArtifactKey(Profile.Tool, Profile.GetGroupOrFallback(GroupFallback), id), cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Attempts to get info for the artifact with the specified ID.
    /// </summary>
    /// <param name="key">Artifact key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning retrieved artifact, if it exists.</returns>
    public async Task<ArtifactInfo?> TryGetArtifactAsync(ArtifactKey key, CancellationToken cancellationToken = default)
        => await RegistrationManager.TryGetArtifactAsync(key, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Attempts to get info for the resource with the specified key.
    /// </summary>
    /// <param name="key">Resource key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning retrieved resource, if it exists.</returns>
    public async Task<ArtifactResourceInfo?> TryGetResourceAsync(ArtifactResourceKey key, CancellationToken cancellationToken = default)
        => await RegistrationManager.TryGetResourceAsync(key, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Tests artifact state against existing.
    /// </summary>
    /// <param name="artifactInfo">Artifact to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning comparison against whatever exists with the same ID.</returns>
    public ValueTask<ItemStateFlags> CompareArtifactAsync(ArtifactInfo artifactInfo, CancellationToken cancellationToken = default)
    {
        return ArtifactToolBaseExtensions.CompareArtifactAsync(this, artifactInfo, cancellationToken);
    }

    /// <summary>
    /// Attempts to get resource with populated version (if available) based on provided resource.
    /// </summary>
    /// <param name="resource">Resource to check.</param>
    /// <param name="resourceUpdate">Resource update mode.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning instance for the resource with populated version (if available), and additional state information.</returns>
    public Task<ArtifactResourceInfoWithState> DetermineUpdatedResourceAsync(ArtifactResourceInfo resource, ResourceUpdateMode resourceUpdate, CancellationToken cancellationToken = default)
    {
        return ArtifactToolBaseExtensions.DetermineUpdatedResourceAsync(this, resource, resourceUpdate, cancellationToken);
    }

    #endregion
}

internal partial class ArtifactToolBaseExtensions
{
    public static async ValueTask<ItemStateFlags> CompareArtifactAsync(this IArtifactTool artifactTool, ArtifactInfo artifactInfo, CancellationToken cancellationToken = default)
    {
        return ItemStateFlagsUtility.GetItemStateFlags(await artifactTool.RegistrationManager.TryGetArtifactAsync(artifactInfo.Key, cancellationToken).ConfigureAwait(false), artifactInfo);
    }

    public static async Task<ArtifactResourceInfoWithState> DetermineUpdatedResourceAsync(this IArtifactTool artifactTool, ArtifactResourceInfo resource, ResourceUpdateMode resourceUpdate, CancellationToken cancellationToken = default)
    {
        ItemStateFlags initialState = resourceUpdate switch
        {
            ResourceUpdateMode.ArtifactSoft => ItemStateFlags.None,
            ResourceUpdateMode.ArtifactHard => ItemStateFlags.EnforceNew,
            ResourceUpdateMode.Soft => ItemStateFlags.None,
            ResourceUpdateMode.Hard => ItemStateFlags.EnforceNew,
            _ => throw new ArgumentOutOfRangeException(nameof(resourceUpdate), resourceUpdate, null)
        };
        resource = await resource.WithMetadataAsync(cancellationToken).ConfigureAwait(false);
        var state = initialState | ItemStateFlagsUtility.GetItemStateFlags(await artifactTool.RegistrationManager.TryGetResourceAsync(resource.Key, cancellationToken).ConfigureAwait(false), resource);
        return new ArtifactResourceInfoWithState(resource, state);
    }
}

/// <summary>
/// Utility for working with item state flags.
/// </summary>
public static class ItemStateFlagsUtility
{
    /// <summary>
    /// Evaluates item state flags.
    /// </summary>
    /// <param name="previousArtifactInfo">Previous artifact if one exists.</param>
    /// <param name="artifactInfo">Current artifact.</param>
    /// <returns>Flags.</returns>
    public static ItemStateFlags GetItemStateFlags(ArtifactInfo? previousArtifactInfo, ArtifactInfo artifactInfo)
    {
        ItemStateFlags state = ItemStateFlags.None;
        if (previousArtifactInfo == null)
        {
            state |= ItemStateFlags.New;
            if (artifactInfo.Date != null || artifactInfo.UpdateDate != null)
                state |= ItemStateFlags.NewerDate;
        }
        else
        {
            if (artifactInfo.UpdateDate != null && previousArtifactInfo.UpdateDate != null)
            {
                if (artifactInfo.UpdateDate > previousArtifactInfo.UpdateDate)
                    state |= ItemStateFlags.NewerDate;
                else if (artifactInfo.UpdateDate < previousArtifactInfo.UpdateDate)
                    state |= ItemStateFlags.OlderDate;
            }
            else if (artifactInfo.UpdateDate != null && previousArtifactInfo.UpdateDate == null)
                state |= ItemStateFlags.NewerDate;
            else if (artifactInfo.UpdateDate == null && previousArtifactInfo.UpdateDate == null)
            {
                if (artifactInfo.Date != null && previousArtifactInfo.Date != null)
                {
                    if (artifactInfo.Date > previousArtifactInfo.Date)
                        state |= ItemStateFlags.NewerDate;
                    else if (artifactInfo.Date < previousArtifactInfo.Date)
                        state |= ItemStateFlags.OlderDate;
                }
                else if (artifactInfo.Date != null && previousArtifactInfo.Date == null)
                    state |= ItemStateFlags.NewerDate;
            }
            if (artifactInfo.Full && !previousArtifactInfo.Full)
                state |= ItemStateFlags.New;
        }
        return state;
    }

    /// <summary>
    /// Evaluates item state flags.
    /// </summary>
    /// <param name="previousResource">Previous resource if one exists.</param>
    /// <param name="resource">Current resource.</param>
    /// <returns></returns>
    public static ItemStateFlags GetItemStateFlags(ArtifactResourceInfo? previousResource, ArtifactResourceInfo resource)
    {
        var state = ItemStateFlags.None;
        if (previousResource == null)
        {
            state |= ItemStateFlags.ChangedMetadata | ItemStateFlags.New;
            if (resource.Updated != null)
                state |= ItemStateFlags.NewerDate;
            if (resource.Version != null)
                state |= ItemStateFlags.ChangedVersion;
            if (resource.Checksum != null && !ChecksumUtility.DatawiseEquals(resource.Checksum, null))
                state |= ItemStateFlags.NewChecksum;
        }
        else
        {
            if (resource.Version != previousResource.Version)
                state |= ItemStateFlags.ChangedVersion;
            if (resource.Updated > previousResource.Updated)
                state |= ItemStateFlags.NewerDate;
            else if (resource.Updated < previousResource.Updated)
                state |= ItemStateFlags.OlderDate;
            if (resource.IsMetadataDifferent(previousResource))
                state |= ItemStateFlags.ChangedMetadata;
            if (resource.Checksum != null && !ChecksumUtility.DatawiseEquals(resource.Checksum, previousResource.Checksum))
                state |= ItemStateFlags.NewChecksum;
        }
        return state;
    }
}
