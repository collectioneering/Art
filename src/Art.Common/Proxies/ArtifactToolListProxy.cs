﻿using System.Runtime.CompilerServices;
using Art.Common.Management;

namespace Art.Common.Proxies;

/// <summary>
/// Proxy to run artifact tool as a list tool.
/// </summary>
public record ArtifactToolListProxy
{
    private const string OptArtifactList = "artifactList";

    /// <summary>Artifact tool.</summary>
    public IArtifactTool ArtifactTool { get; init; }

    /// <summary>List options.</summary>
    public ArtifactToolListOptions Options { get; init; }

    /// <summary>Log handler.</summary>
    public IToolLogHandler? LogHandler { get; init; }

    /// <summary>
    /// Proxy to run artifact tool as a list tool.
    /// </summary>
    /// <param name="artifactTool">Artifact tool.</param>
    /// <param name="options">List options.</param>
    /// <param name="logHandler">Log handler.</param>
    /// <exception cref="ArgumentException">Thrown when invalid options are specified.</exception>
    public ArtifactToolListProxy(IArtifactTool artifactTool, ArtifactToolListOptions options, IToolLogHandler? logHandler)
    {
        if (artifactTool == null) throw new ArgumentNullException(nameof(artifactTool));
        if (options == null) throw new ArgumentNullException(nameof(options));
        ArtifactToolListOptions.Validate(options, false);
        ArtifactTool = artifactTool;
        Options = options;
        LogHandler = logHandler;
    }

    #region API

    /// <summary>
    /// Lists artifacts.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async-enumerable artifacts.</returns>
    /// <exception cref="InvalidOperationException">Thrown when an invalid configuration is detected.</exception>
    /// <exception cref="NotSupportedException">Thrown when a tool does not natively and cannot be made to support listing.</exception>
    public async IAsyncEnumerable<IArtifactData> ListAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (ArtifactTool == null) throw new InvalidOperationException("Artifact tool cannot be null");
        if (Options == null) throw new InvalidOperationException("Options cannot be null");
        ArtifactToolListOptions.Validate(Options, false);
        IArtifactTool artifactTool = ArtifactTool;
        if (artifactTool.Profile.Options.TryGetOption(OptArtifactList, out string[]? artifactList, SourceGenerationContext.s_context.StringArray) && artifactTool is IArtifactFindTool findTool)
        {
            artifactTool = new FindAsListTool(findTool, artifactList);
        }
        if (LogHandler != null) artifactTool.LogHandler = LogHandler;
        if (artifactTool is IArtifactListTool listTool)
        {
            IAsyncEnumerable<IArtifactData> enumerable = listTool.ListAsync(cancellationToken);
            if ((Options.EagerFlags & artifactTool.AllowedEagerModes & EagerFlags.ArtifactList) != 0) enumerable = enumerable.EagerAsync();
            await foreach (IArtifactData data in enumerable.ConfigureAwait(false))
            {
                switch (Options.SkipMode)
                {
                    case ArtifactSkipMode.None:
                        break;
                    case ArtifactSkipMode.FastExit:
                        {
                            ArtifactInfo? info = await artifactTool.RegistrationManager.TryGetArtifactAsync(data.Info.Key, cancellationToken).ConfigureAwait(false);
                            if (info != null)
                                yield break;
                            break;
                        }
                    case ArtifactSkipMode.Known:
                        {
                            ArtifactInfo? info = await artifactTool.RegistrationManager.TryGetArtifactAsync(data.Info.Key, cancellationToken).ConfigureAwait(false);
                            if (info != null)
                                continue;
                            break;
                        }
                }
                if (!data.Info.Full && !Options.IncludeNonFull) continue;
                yield return data;
            }
            yield break;
        }
        if (artifactTool is IArtifactDumpTool dumpTool)
        {
            IArtifactDataManager previousAdm = artifactTool.DataManager;
            IArtifactRegistrationManager previousArm = artifactTool.RegistrationManager;
            try
            {
                InMemoryArtifactDataManager adm = new();
                InMemoryArtifactRegistrationManager arm = new();
                artifactTool.DataManager = adm;
                artifactTool.RegistrationManager = arm;
                await dumpTool.DumpAsync(cancellationToken).ConfigureAwait(false);
                HashSet<ArtifactKey> known = new();
                foreach ((ArtifactKey ak, List<ArtifactResourceInfo> resources) in adm.Artifacts)
                {
                    if (await artifactTool.RegistrationManager.TryGetArtifactAsync(ak, cancellationToken).ConfigureAwait(false) is not { } info)
                    {
                        continue;
                    }
                    if (!known.Add(ak))
                    {
                        continue;
                    }
                    ArtifactData data = new(info);
                    data.AddRange(resources);
                    yield return data;
                }
                foreach (var info in await arm.ListArtifactsAsync(cancellationToken).ConfigureAwait(false))
                {
                    if (!known.Add(info.Key))
                    {
                        continue;
                    }
                    yield return new ArtifactData(info);
                }
            }
            finally
            {
                artifactTool.DataManager = previousAdm;
                artifactTool.RegistrationManager = previousArm;
            }
            yield break;
        }
        throw new NotSupportedException("Artifact tool is not a supported type");
    }

    #endregion
}
