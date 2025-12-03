using System.Diagnostics.CodeAnalysis;
using System.Runtime.Loader;
using Art.Common.Management;
using Art.Common.Proxies;

namespace Art.Common;

/// <summary>
/// Simple dumping process.
/// </summary>
public static class ArtifactDumping
{
    /// <summary>
    /// Directly dumps using a tool profile stored on disk.
    /// </summary>
    /// <param name="artifactToolProfilePath">Path to tool profile.</param>
    /// <param name="targetDirectory">Base directory.</param>
    /// <param name="timeProvider">Time provider.</param>
    /// <param name="getArtifactRetrievalTimestamps">Get artifact retrieval timestamps.</param>
    /// <param name="getResourceRetrievalTimestamps">Get resource retrieval timestamps.</param>
    /// <param name="dumpOptions">Dump options.</param>
    /// <param name="toolLogHandler">Tool log handler.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <exception cref="ArtifactToolNotFoundException">Thrown when tool is not found.</exception>
    [RequiresUnreferencedCode($"Loading artifact tools might require types that cannot be statically analyzed. Consider making use of the overload that takes {nameof(IArtifactToolRegistry)} where possible.")]
    public static async Task DumpAsync(
        string artifactToolProfilePath,
        string targetDirectory,
        TimeProvider timeProvider,
        bool getArtifactRetrievalTimestamps,
        bool getResourceRetrievalTimestamps,
        ArtifactToolDumpOptions? dumpOptions = null,
        IToolLogHandler? toolLogHandler = null,
        CancellationToken cancellationToken = default)
    {
        dumpOptions ??= new ArtifactToolDumpOptions();
        using var srm = new DiskArtifactRegistrationManager(targetDirectory);
        using var sdm = new DiskArtifactDataManager(targetDirectory);
        foreach (ArtifactToolProfile profile in ArtifactToolProfileUtil.DeserializeProfilesFromFile(artifactToolProfilePath))
            await DumpAsync(profile, srm, sdm, timeProvider, getArtifactRetrievalTimestamps, getResourceRetrievalTimestamps, dumpOptions, toolLogHandler, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Directly dumps using a tool profile stored on disk.
    /// </summary>
    /// <param name="assemblyLoadContext">Custom <see cref="AssemblyLoadContext"/>.</param>
    /// <param name="artifactToolProfilePath">Path to tool profile.</param>
    /// <param name="targetDirectory">Base directory.</param>
    /// <param name="timeProvider">Time provider.</param>
    /// <param name="getArtifactRetrievalTimestamps">Get artifact retrieval timestamps.</param>
    /// <param name="getResourceRetrievalTimestamps">Get resource retrieval timestamps.</param>
    /// <param name="dumpOptions">Dump options.</param>
    /// <param name="toolLogHandler">Tool log handler.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <exception cref="ArtifactToolNotFoundException">Thrown when tool is not found.</exception>
    [RequiresUnreferencedCode($"Loading artifact tools might require types that cannot be statically analyzed. Consider making use of the overload that takes {nameof(IArtifactToolRegistry)} where possible.")]
    public static async Task DumpAsync(
        AssemblyLoadContext assemblyLoadContext,
        string artifactToolProfilePath,
        string targetDirectory,
        TimeProvider timeProvider,
        bool getArtifactRetrievalTimestamps,
        bool getResourceRetrievalTimestamps,
        ArtifactToolDumpOptions? dumpOptions = null,
        IToolLogHandler? toolLogHandler = null,
        CancellationToken cancellationToken = default)
    {
        dumpOptions ??= new ArtifactToolDumpOptions();
        using var srm = new DiskArtifactRegistrationManager(targetDirectory);
        using var sdm = new DiskArtifactDataManager(targetDirectory);
        foreach (ArtifactToolProfile profile in ArtifactToolProfileUtil.DeserializeProfilesFromFile(artifactToolProfilePath))
            await DumpAsync(assemblyLoadContext, profile, srm, sdm, timeProvider, getArtifactRetrievalTimestamps, getResourceRetrievalTimestamps, dumpOptions, toolLogHandler, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Directly dumps using a tool profile stored on disk.
    /// </summary>
    /// <param name="artifactToolRegistry">Custom <see cref="IArtifactToolRegistry"/>.</param>
    /// <param name="artifactToolProfilePath">Path to tool profile.</param>
    /// <param name="targetDirectory">Base directory.</param>
    /// <param name="timeProvider">Time provider.</param>
    /// <param name="getArtifactRetrievalTimestamps">Get artifact retrieval timestamps.</param>
    /// <param name="getResourceRetrievalTimestamps">Get resource retrieval timestamps.</param>
    /// <param name="dumpOptions">Dump options.</param>
    /// <param name="toolLogHandler">Tool log handler.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <exception cref="ArtifactToolNotFoundException">Thrown when tool is not found.</exception>
    public static async Task DumpAsync(
        IArtifactToolRegistry artifactToolRegistry,
        string artifactToolProfilePath,
        string targetDirectory,
        TimeProvider timeProvider,
        bool getArtifactRetrievalTimestamps,
        bool getResourceRetrievalTimestamps,
        ArtifactToolDumpOptions? dumpOptions = null,
        IToolLogHandler? toolLogHandler = null,
        CancellationToken cancellationToken = default)
    {
        dumpOptions ??= new ArtifactToolDumpOptions();
        using var srm = new DiskArtifactRegistrationManager(targetDirectory);
        using var sdm = new DiskArtifactDataManager(targetDirectory);
        foreach (ArtifactToolProfile profile in ArtifactToolProfileUtil.DeserializeProfilesFromFile(artifactToolProfilePath))
            await DumpAsync(artifactToolRegistry, profile, srm, sdm, timeProvider, getArtifactRetrievalTimestamps, getResourceRetrievalTimestamps, dumpOptions, toolLogHandler, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Directly dumps to disk using a tool profile.
    /// </summary>
    /// <param name="artifactToolProfile">Tool profile.</param>
    /// <param name="artifactRegistrationManager">Registration manager.</param>
    /// <param name="artifactDataManager">Data manager.</param>
    /// <param name="timeProvider">Time provider.</param>
    /// <param name="getArtifactRetrievalTimestamps">Get artifact retrieval timestamps.</param>
    /// <param name="getResourceRetrievalTimestamps">Get resource retrieval timestamps.</param>
    /// <param name="dumpOptions">Dump options.</param>
    /// <param name="toolLogHandler">Tool log handler.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <exception cref="ArgumentException">Thrown when an invalid profile is provided.</exception>
    /// <exception cref="ArtifactToolNotFoundException">Thrown when tool is not found.</exception>
    [RequiresUnreferencedCode($"Loading artifact tools might require types that cannot be statically analyzed. Consider making use of the overload that takes {nameof(IArtifactToolRegistry)} where possible.")]
    public static async Task DumpAsync(
        ArtifactToolProfile artifactToolProfile,
        IArtifactRegistrationManager artifactRegistrationManager,
        IArtifactDataManager artifactDataManager,
        TimeProvider timeProvider,
        bool getArtifactRetrievalTimestamps,
        bool getResourceRetrievalTimestamps,
        ArtifactToolDumpOptions? dumpOptions = null,
        IToolLogHandler? toolLogHandler = null,
        CancellationToken cancellationToken = default)
    {
        using IArtifactTool tool = await ArtifactTool.PrepareToolAsync(artifactToolProfile, artifactRegistrationManager, artifactDataManager, timeProvider, getArtifactRetrievalTimestamps, getResourceRetrievalTimestamps, cancellationToken).ConfigureAwait(false);
        await new ArtifactToolDumpProxy(tool, dumpOptions ?? new ArtifactToolDumpOptions(), toolLogHandler).DumpAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Directly dumps to disk using a tool profile.
    /// </summary>
    /// <param name="assemblyLoadContext">Custom <see cref="AssemblyLoadContext"/>.</param>
    /// <param name="artifactToolProfile">Tool profile.</param>
    /// <param name="artifactRegistrationManager">Registration manager.</param>
    /// <param name="artifactDataManager">Data manager.</param>
    /// <param name="timeProvider">Time provider.</param>
    /// <param name="getArtifactRetrievalTimestamps">Get artifact retrieval timestamps.</param>
    /// <param name="getResourceRetrievalTimestamps">Get resource retrieval timestamps.</param>
    /// <param name="dumpOptions">Dump options.</param>
    /// <param name="toolLogHandler">Tool log handler.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <exception cref="ArgumentException">Thrown when an invalid profile is provided.</exception>
    /// <exception cref="ArtifactToolNotFoundException">Thrown when tool is not found.</exception>
    [RequiresUnreferencedCode($"Loading artifact tools might require types that cannot be statically analyzed. Consider making use of the overload that takes {nameof(IArtifactToolRegistry)} where possible.")]
    public static async Task DumpAsync(
        AssemblyLoadContext assemblyLoadContext,
        ArtifactToolProfile artifactToolProfile,
        IArtifactRegistrationManager artifactRegistrationManager,
        IArtifactDataManager artifactDataManager,
        TimeProvider timeProvider,
        bool getArtifactRetrievalTimestamps,
        bool getResourceRetrievalTimestamps,
        ArtifactToolDumpOptions? dumpOptions = null,
        IToolLogHandler? toolLogHandler = null,
        CancellationToken cancellationToken = default)
    {
        using IArtifactTool tool = await ArtifactTool.PrepareToolAsync(assemblyLoadContext, artifactToolProfile, artifactRegistrationManager, artifactDataManager, timeProvider, getArtifactRetrievalTimestamps, getResourceRetrievalTimestamps, cancellationToken).ConfigureAwait(false);
        await new ArtifactToolDumpProxy(tool, dumpOptions ?? new ArtifactToolDumpOptions(), toolLogHandler).DumpAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Directly dumps to disk using a tool profile.
    /// </summary>
    /// <param name="artifactToolRegistry">Custom <see cref="IArtifactToolRegistry"/>.</param>
    /// <param name="artifactToolProfile">Tool profile.</param>
    /// <param name="artifactRegistrationManager">Registration manager.</param>
    /// <param name="artifactDataManager">Data manager.</param>
    /// <param name="timeProvider">Time provider.</param>
    /// <param name="getArtifactRetrievalTimestamps">Get artifact retrieval timestamps.</param>
    /// <param name="getResourceRetrievalTimestamps">Get resource retrieval timestamps.</param>
    /// <param name="dumpOptions">Dump options.</param>
    /// <param name="toolLogHandler">Tool log handler.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <exception cref="ArgumentException">Thrown when an invalid profile is provided.</exception>
    /// <exception cref="ArtifactToolNotFoundException">Thrown when tool is not found.</exception>
    public static async Task DumpAsync(
        IArtifactToolRegistry artifactToolRegistry,
        ArtifactToolProfile artifactToolProfile,
        IArtifactRegistrationManager artifactRegistrationManager,
        IArtifactDataManager artifactDataManager,
        TimeProvider timeProvider,
        bool getArtifactRetrievalTimestamps,
        bool getResourceRetrievalTimestamps,
        ArtifactToolDumpOptions? dumpOptions = null,
        IToolLogHandler? toolLogHandler = null,
        CancellationToken cancellationToken = default)
    {
        using IArtifactTool tool = await ArtifactTool.PrepareToolAsync(artifactToolRegistry, artifactToolProfile, artifactRegistrationManager, artifactDataManager, timeProvider, getArtifactRetrievalTimestamps, getResourceRetrievalTimestamps, cancellationToken).ConfigureAwait(false);
        await new ArtifactToolDumpProxy(tool, dumpOptions ?? new ArtifactToolDumpOptions(), toolLogHandler).DumpAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Directly dumps to disk using a tool profile.
    /// </summary>
    /// <typeparam name="T">Artifact tool factory type.</typeparam>
    /// <param name="artifactToolProfile">Tool profile.</param>
    /// <param name="artifactRegistrationManager">Registration manager.</param>
    /// <param name="artifactDataManager">Data manager.</param>
    /// <param name="timeProvider">Time provider.</param>
    /// <param name="getArtifactRetrievalTimestamps">Get artifact retrieval timestamps.</param>
    /// <param name="getResourceRetrievalTimestamps">Get resource retrieval timestamps.</param>
    /// <param name="dumpOptions">Dump options.</param>
    /// <param name="toolLogHandler">Tool log handler.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <exception cref="ArgumentException">Thrown when an invalid profile is provided.</exception>
    /// <exception cref="ArtifactToolNotFoundException">Thrown when tool is not found.</exception>
    public static async Task DumpAsync<T>(
        ArtifactToolProfile artifactToolProfile,
        IArtifactRegistrationManager artifactRegistrationManager,
        IArtifactDataManager artifactDataManager,
        TimeProvider timeProvider,
        bool getArtifactRetrievalTimestamps,
        bool getResourceRetrievalTimestamps,
        ArtifactToolDumpOptions? dumpOptions = null,
        IToolLogHandler? toolLogHandler = null,
        CancellationToken cancellationToken = default) where T : IArtifactToolFactory
    {
        using IArtifactTool tool = await ArtifactTool.PrepareToolAsync<T>(artifactToolProfile, artifactRegistrationManager, artifactDataManager, timeProvider, getArtifactRetrievalTimestamps, getResourceRetrievalTimestamps, cancellationToken).ConfigureAwait(false);
        await new ArtifactToolDumpProxy(tool, dumpOptions ?? new ArtifactToolDumpOptions(), toolLogHandler).DumpAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Dumps an artifact asynchronously.
    /// </summary>
    /// <param name="artifactTool">Origin artifact tool.</param>
    /// <param name="artifactData">Artifact data to dump.</param>
    /// <param name="resourceUpdate">Resource update mode.</param>
    /// <param name="logHandler">Log handler.</param>
    /// <param name="checksumSource">Optional checksum source, if resources are to have their checksums computed.</param>
    /// <param name="timeProvider">Time provider.</param>
    /// <param name="getArtifactRetrievalTimestamps">Get artifact retrieval timestamps.</param>
    /// <param name="getResourceRetrievalTimestamps">Get resource retrieval timestamps.</param>
    /// <param name="eagerFlags">Eager flags.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="resourceUpdate"/> is invalid.</exception>
    public static async Task DumpArtifactAsync(
        this IArtifactTool artifactTool,
        IArtifactData artifactData,
        ResourceUpdateMode resourceUpdate = ResourceUpdateMode.Soft,
        IToolLogHandler? logHandler = null,
        ChecksumSource? checksumSource = null,
        TimeProvider? timeProvider = null,
        bool? getArtifactRetrievalTimestamps = null,
        bool? getResourceRetrievalTimestamps = null,
        EagerFlags eagerFlags = EagerFlags.None,
        CancellationToken cancellationToken = default)
    {
        switch (resourceUpdate)
        {
            case ResourceUpdateMode.ArtifactSoft:
            case ResourceUpdateMode.ArtifactHard:
            case ResourceUpdateMode.Soft:
            case ResourceUpdateMode.Hard:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(resourceUpdate));
        }
        ArtifactInfo artifactInfo = artifactData.Info;
        if ((getArtifactRetrievalTimestamps ?? false) && timeProvider != null && artifactInfo.RetrievalDate == null)
        {
            artifactInfo = artifactInfo with { RetrievalDate = timeProvider.GetUtcNow() };
        }
        ItemStateFlags iF = await artifactTool.CompareArtifactAsync(artifactInfo, cancellationToken).ConfigureAwait(false);
        logHandler?.Log(artifactTool.Profile.Tool, artifactTool.Profile.GetGroupOrFallback(artifactInfo.Key.Group), $"{((iF & ItemStateFlags.NewerIdentityMask) != 0 ? "[NEW] " : "")}{artifactInfo.GetInfoTitleString()}", artifactInfo.GetInfoString(), LogLevel.Entry);
        if ((iF & ItemStateFlags.NewerIdentityMask) != 0)
            await artifactTool.RegistrationManager.AddArtifactAsync(artifactInfo with { Full = false }, cancellationToken).ConfigureAwait(false);
        switch (resourceUpdate)
        {
            case ResourceUpdateMode.ArtifactSoft:
            case ResourceUpdateMode.ArtifactHard:
                if ((iF & ItemStateFlags.NewerIdentityMask) == 0) goto SaveArtifact;
                break;
            case ResourceUpdateMode.Soft:
            case ResourceUpdateMode.Hard:
                break;
        }
        switch (eagerFlags & artifactTool.AllowedEagerModes & (EagerFlags.ResourceMetadata | EagerFlags.ResourceObtain))
        {
            case EagerFlags.ResourceMetadata | EagerFlags.ResourceObtain:
                {
                    Task[] tasks = artifactData.Values.Select(async v =>
                        await UpdateResourceAsync(artifactTool, await artifactTool.DetermineUpdatedResourceAsync(v, resourceUpdate, cancellationToken).ConfigureAwait(false), logHandler, checksumSource, timeProvider, getResourceRetrievalTimestamps, cancellationToken).ConfigureAwait(false)).ToArray();
                    await Task.WhenAll(tasks).ConfigureAwait(false);
                    break;
                }
            case EagerFlags.ResourceMetadata:
                Task<ArtifactResourceInfoWithState>[] updateTasks = artifactData.Values.Select(v => artifactTool.DetermineUpdatedResourceAsync(v, resourceUpdate, cancellationToken)).ToArray();
                ArtifactResourceInfoWithState[] items = await Task.WhenAll(updateTasks).ConfigureAwait(false);
                foreach (ArtifactResourceInfoWithState aris in items)
                    await UpdateResourceAsync(artifactTool, aris, logHandler, checksumSource, timeProvider, getResourceRetrievalTimestamps, cancellationToken).ConfigureAwait(false);
                break;
            case EagerFlags.ResourceObtain:
                {
                    List<Task> tasks = new();
                    foreach (ArtifactResourceInfo resource in artifactData.Values)
                    {
                        ArtifactResourceInfoWithState aris = await artifactTool.DetermineUpdatedResourceAsync(resource, resourceUpdate, cancellationToken).ConfigureAwait(false);
                        tasks.Add(UpdateResourceAsync(artifactTool, aris, logHandler, checksumSource, timeProvider, getResourceRetrievalTimestamps, cancellationToken));
                    }
                    await Task.WhenAll(tasks).ConfigureAwait(false);
                    break;
                }
            default:
                {
                    foreach (ArtifactResourceInfo resource in artifactData.Values)
                        await UpdateResourceAsync(artifactTool, await artifactTool.DetermineUpdatedResourceAsync(resource, resourceUpdate, cancellationToken).ConfigureAwait(false), logHandler, checksumSource, timeProvider, getResourceRetrievalTimestamps, cancellationToken).ConfigureAwait(false);
                    break;
                }
        }
        SaveArtifact:
        if ((iF & ItemStateFlags.NewerIdentityMask) != 0)
            await artifactTool.RegistrationManager.AddArtifactAsync(artifactInfo, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Dumps a resource asynchronously.
    /// </summary>
    /// <param name="artifactTool">Artifact tool.</param>
    /// <param name="resource">Resource to check.</param>
    /// <param name="resourceUpdate">Resource update mode.</param>
    /// <param name="logHandler">Log handler.</param>
    /// <param name="checksumSource">Optional checksum source, if resource is to have its checksum computed.</param>
    /// <param name="timeProvider">Time provider.</param>
    /// <param name="getResourceRetrievalTimestamps">Get resource retrieval timestamps.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static async Task DumpResourceAsync(
        this IArtifactTool artifactTool,
        ArtifactResourceInfo resource,
        ResourceUpdateMode resourceUpdate,
        IToolLogHandler? logHandler,
        ChecksumSource? checksumSource,
        TimeProvider? timeProvider,
        bool? getResourceRetrievalTimestamps,
        CancellationToken cancellationToken = default)
    {
        await UpdateResourceAsync(artifactTool, await artifactTool.DetermineUpdatedResourceAsync(resource, resourceUpdate, cancellationToken).ConfigureAwait(false), logHandler, checksumSource, timeProvider, getResourceRetrievalTimestamps, cancellationToken).ConfigureAwait(false);
    }

    private static bool GetResourceRetrievable(ArtifactResourceInfo resource)
    {
        return resource.CanExportStream || resource.CanGetStream;
    }

    private static async Task CopyResourceAsync(ArtifactResourceInfo resource, Stream targetStream, CancellationToken cancellationToken)
    {
        if (resource.CanExportStream)
        {
            await resource.ExportStreamAsync(targetStream, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        else if (resource.CanGetStream)
        {
            await using var resourceStream = await resource.GetStreamAsync(cancellationToken).ConfigureAwait(false);
            await resourceStream.CopyToAsync(targetStream, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            throw new InvalidOperationException("Cannot copy stream from resource that does not support export or raw stream");
        }
    }

    private static async Task UpdateResourceAsync(
        IArtifactTool artifactTool,
        ArtifactResourceInfoWithState aris,
        IToolLogHandler? logHandler,
        ChecksumSource? checksumSource,
        TimeProvider? timeProvider,
        bool? getResourceRetrievalTimestamps,
        CancellationToken cancellationToken)
    {
        (ArtifactResourceInfo versionedResource, ItemStateFlags rF) = aris;
        if ((rF & ItemStateFlags.NewerIdentityMask) != 0 && GetResourceRetrievable(versionedResource))
        {
            OutputStreamOptions options = OutputStreamOptions.Default;
            if ((getResourceRetrievalTimestamps ?? false) && timeProvider != null && versionedResource.Retrieved == null)
            {
                versionedResource = versionedResource with { Retrieved = timeProvider.GetUtcNow() };
            }
            versionedResource.AugmentOutputStreamOptions(ref options);
            await using CommittableStream stream = await artifactTool.DataManager.CreateOutputStreamAsync(versionedResource.Key, options, cancellationToken).ConfigureAwait(false);
            if (checksumSource != null)
            {
                using var algorithm = checksumSource.CreateHashAlgorithm();
                // Take this opportunity to hash the resource.
                await using HashProxyStream hps = new(stream, algorithm, true, true);
                await CopyResourceAsync(versionedResource, hps, cancellationToken).ConfigureAwait(false);
                stream.ShouldCommit = true;
                Checksum newChecksum = new(checksumSource.Id, hps.GetHash());
                if (!ChecksumUtility.DatawiseEquals(newChecksum, versionedResource.Checksum))
                {
                    rF |= ItemStateFlags.NewChecksum;
                    versionedResource = versionedResource with { Checksum = newChecksum };
                }
            }
            else if (stream is not ISinkStream) // if target output were a sink stream and hash isn't needed, then just don't bother exporting
            {
                await CopyResourceAsync(versionedResource, stream, cancellationToken).ConfigureAwait(false);
                stream.ShouldCommit = true;
            }
        }
        logHandler?.Log(artifactTool.Profile.Tool, artifactTool.Profile.GetGroupOrFallback(aris.ArtifactResourceInfo.Key.Artifact.Group), $"-- {((rF & ItemStateFlags.NewerIdentityMask) != 0 ? "[NEW] " : "")}{versionedResource.GetInfoPathString()}", versionedResource.GetInfoString(), LogLevel.Entry);
        if ((rF & ItemStateFlags.DifferentMask) != 0)
            await artifactTool.RegistrationManager.AddResourceAsync(versionedResource, cancellationToken).ConfigureAwait(false);
    }
}
