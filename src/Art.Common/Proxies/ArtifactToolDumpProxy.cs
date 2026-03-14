namespace Art.Common.Proxies;

/// <summary>
/// Proxy to run artifact tool as a dump tool.
/// </summary>
public record ArtifactToolDumpProxy
{
    private const string OptArtifactList = "artifactList";

    /// <summary>Artifact tool.</summary>
    public IArtifactTool ArtifactTool { get; init; }

    /// <summary>Dump options.</summary>
    public ArtifactToolDumpOptions Options { get; init; }

    /// <summary>Log handler.</summary>
    public IToolLogHandler? LogHandler { get; init; }

    /// <summary>
    /// Proxy to run artifact tool as a dump tool.
    /// </summary>
    /// <param name="artifactTool">Artifact tool.</param>
    /// <param name="options">Dump options.</param>
    /// <param name="logHandler">Log handler.</param>
    public ArtifactToolDumpProxy(
        IArtifactTool artifactTool,
        ArtifactToolDumpOptions options,
        IToolLogHandler? logHandler)
    {
        ArtifactTool = artifactTool ?? throw new ArgumentNullException(nameof(artifactTool));
        Options = options ?? throw new ArgumentNullException(nameof(options));
        Validate(options, true);
        LogHandler = logHandler;
    }

    private static void Validate(ArtifactToolDumpOptions options, bool constructor)
    {
        ArtifactToolDumpOptions.Validate(options, constructor);
        if (options.SkipMode == ArtifactSkipMode.FastExit && (options.EagerFlags & EagerFlags.ArtifactDump) != 0)
        {
            if (constructor)
            {
                throw new ArgumentException($"Cannot pair {nameof(ArtifactSkipMode)}.{nameof(ArtifactSkipMode.FastExit)} with {nameof(EagerFlags)}.{nameof(EagerFlags.ArtifactDump)}");
            }
            throw new InvalidOperationException($"Cannot pair {nameof(ArtifactSkipMode)}.{nameof(ArtifactSkipMode.FastExit)} with {nameof(EagerFlags)}.{nameof(EagerFlags.ArtifactDump)}");
        }
    }

    /// <summary>
    /// Dumps artifacts.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <exception cref="InvalidOperationException">Thrown when an invalid configuration is detected.</exception>
    /// <exception cref="NotSupportedException">Thrown when a tool does not natively and cannot be made to support dumping.</exception>
    public async ValueTask DumpAsync(CancellationToken cancellationToken = default)
    {
        if (ArtifactTool == null) throw new InvalidOperationException("Artifact tool cannot be null");
        if (Options == null) throw new InvalidOperationException("Options cannot be null");
        Validate(Options, false);
        IArtifactTool artifactTool = ArtifactTool;
        if (artifactTool.Profile.Options.TryGetOption(OptArtifactList, out string[]? artifactList, SourceGenerationContext.s_context.StringArray) && artifactTool is IArtifactFindTool findTool)
        {
            artifactTool = new FindAsListTool(findTool, artifactList);
        }
        var existingLogHandler = artifactTool.LogHandler;
        try
        {
            if (LogHandler != null)
            {
                artifactTool.LogHandler = LogHandler;
            }
            if (artifactTool is IArtifactDumpTool dumpTool)
            {
                await dumpTool.DumpAsync(cancellationToken).ConfigureAwait(false);
                return;
            }
            if (artifactTool is IArtifactListTool listTool)
            {
                IAsyncEnumerable<IArtifactData> enumerable = listTool.ListAsync(cancellationToken);
                if ((Options.EagerFlags & artifactTool.AllowedEagerModes & EagerFlags.ArtifactList) != 0) enumerable = enumerable.EagerAsync();
                if ((Options.EagerFlags & artifactTool.AllowedEagerModes & EagerFlags.ArtifactDump) != 0)
                {
                    List<Task> tasks = new();
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
                                        goto E_ArtifactDump_WaitForTasks;
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
                        tasks.Add(artifactTool.DumpArtifactAsync(
                            data,
                            Options.ResourceUpdate,
                            LogHandler,
                            Options.ChecksumSource,
                            artifactTool.TimeProvider,
                            artifactTool.Config.GetArtifactRetrievalTimestamps,
                            artifactTool.Config.GetResourceRetrievalTimestamps,
                            Options.EagerFlags,
                            cancellationToken));
                    }
                    E_ArtifactDump_WaitForTasks:
                    await Task.WhenAll(tasks).ConfigureAwait(false);
                    var exc = tasks
                        .Where(v => v.IsFaulted && v.Exception != null)
                        .SelectMany(v => v.Exception!.InnerExceptions)
                        .ToList();
                    List<Exception> failed;
                    if (Options.IgnoreException is { } ignoreException)
                    {
                        var ignored = exc.Where(ignoreException).ToList();
                        foreach (var ignore in ignored)
                            LogHandler?.Log($"Ignored exception of type {ignore.GetType().FullName}", ignore.ToString(), LogLevel.Warning);
                        failed = exc.Where(v => !ignoreException(v)).ToList();
                    }
                    else
                    {
                        failed = exc;
                    }
                    if (failed.Any())
                        throw new AggregateException(exc);
                }
                else
                {
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
                                        return;
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
                        try
                        {
                            await artifactTool.DumpArtifactAsync(
                                    data,
                                    Options.ResourceUpdate,
                                    LogHandler,
                                    Options.ChecksumSource,
                                    artifactTool.TimeProvider,
                                    artifactTool.Config.GetArtifactRetrievalTimestamps,
                                    artifactTool.Config.GetResourceRetrievalTimestamps,
                                    Options.EagerFlags,
                                    cancellationToken)
                                .ConfigureAwait(false);
                        }
                        catch (Exception e)
                        {
                            if (Options.IgnoreException is { } ignoreException && ignoreException(e))
                                LogHandler?.Log($"Ignored exception of type {e.GetType().FullName}", e.ToString(), LogLevel.Warning);
                            else throw;
                        }
                    }
                }
                return;
            }
            throw new NotSupportedException("Artifact tool is not a supported type");
        }
        finally
        {
            artifactTool.LogHandler = existingLogHandler;
        }
    }
}
