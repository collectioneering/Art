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
        LogHandler = logHandler;
        Validate(this, true);
    }

    private static void Validate(ArtifactToolDumpProxy proxy, bool constructor)
    {
        if (proxy.ArtifactTool == null)
        {
            throw new InvalidOperationException(SharedStrings.ExcArtifactToolCannotBeNull);
        }
        var options = proxy.Options;
        if (options == null)
        {
            throw new InvalidOperationException(SharedStrings.ExcOptionsCannotBeNull);
        }
        ArtifactToolDumpOptions.Validate(options, constructor);
        if ((options.EagerFlags & EagerFlags.ArtifactDump) != 0)
        {
            if (options.SkipMode == ArtifactSkipMode.FastExit)
            {
                const string errorMessage = $"Cannot pair {nameof(ArtifactSkipMode)}.{nameof(ArtifactSkipMode.FastExit)} with {nameof(EagerFlags)}.{nameof(EagerFlags.ArtifactDump)}";
                if (constructor)
                {
                    throw new ArgumentException(errorMessage);
                }
                throw new InvalidOperationException(errorMessage);
            }
            if (options.SkipMode == ArtifactSkipMode.FastExitFull)
            {
                const string errorMessage = $"Cannot pair {nameof(ArtifactSkipMode)}.{nameof(ArtifactSkipMode.FastExitFull)} with {nameof(EagerFlags)}.{nameof(EagerFlags.ArtifactDump)}";
                if (constructor)
                {
                    throw new ArgumentException(errorMessage);
                }
                throw new InvalidOperationException(errorMessage);
            }
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
        Validate(this, false);
        IArtifactTool artifactTool = ArtifactTool;
        if (artifactTool.Profile.Options.TryGetOption(OptArtifactList, out string[]? artifactList, SourceGenerationContext.SharedContext.StringArray) && artifactTool is IArtifactFindTool findTool)
        {
            artifactTool = new FindAsListTool(findTool, artifactList);
        }
        switch (artifactTool)
        {
            case IArtifactDumpTool dumpTool:
                {
                    var existingLogHandler = artifactTool.LogHandler;
                    try
                    {
                        if (LogHandler != null)
                        {
                            artifactTool.LogHandler = LogHandler;
                        }
                        await DumpAsDumpAsync(dumpTool, cancellationToken).ConfigureAwait(false);
                    }
                    finally
                    {
                        artifactTool.LogHandler = existingLogHandler;
                    }
                    break;
                }
            case IArtifactListTool listTool:
                {
                    var existingLogHandler = artifactTool.LogHandler;
                    try
                    {
                        if (LogHandler != null)
                        {
                            artifactTool.LogHandler = LogHandler;
                        }
                        await DumpAsListAsync(listTool, cancellationToken).ConfigureAwait(false);
                    }
                    finally
                    {
                        artifactTool.LogHandler = existingLogHandler;
                    }
                    break;
                }
            default:
                throw new NotSupportedException("Artifact tool is not a supported type");
        }
    }

    private async ValueTask DumpAsDumpAsync(IArtifactDumpTool artifactTool, CancellationToken cancellationToken = default)
    {
        await artifactTool.DumpAsync(cancellationToken).ConfigureAwait(false);
    }

    private class DumpContext
    {
        private readonly IArtifactTool _artifactTool;
        private bool _fastExitFlagged;

        public DumpContext(IArtifactTool artifactTool)
        {
            _artifactTool = artifactTool;
        }

        public async ValueTask<ListFilterCommand> FilterKnownAsync(ArtifactKey artifactKey, CancellationToken cancellationToken = default)
        {
            ArtifactInfo? info = await _artifactTool.RegistrationManager.TryGetArtifactAsync(artifactKey, cancellationToken).ConfigureAwait(false);
            if (info != null)
            {
                return ListFilterCommand.Reject;
            }
            return ListFilterCommand.Accept;
        }

        public async ValueTask<ListFilterCommand> FilterKnownFullAsync(ArtifactKey artifactKey, CancellationToken cancellationToken = default)
        {
            ArtifactInfo? info = await _artifactTool.RegistrationManager.TryGetArtifactAsync(artifactKey, cancellationToken).ConfigureAwait(false);
            if (info != null)
            {
                return ListFilterCommand.Reject;
            }
            return ListFilterCommand.Accept;
        }

        public async ValueTask<ListFilterCommand> FilterFastExitAsync(ArtifactKey artifactKey, CancellationToken cancellationToken = default)
        {
            if (_fastExitFlagged)
            {
                return ListFilterCommand.Abort;
            }
            ArtifactInfo? info = await _artifactTool.RegistrationManager.TryGetArtifactAsync(artifactKey, cancellationToken).ConfigureAwait(false);
            if (info != null)
            {
                _fastExitFlagged = true;
                return ListFilterCommand.Abort;
            }
            return ListFilterCommand.Accept;
        }

        public async ValueTask<ListFilterCommand> FilterFastExitFullAsync(ArtifactKey artifactKey, CancellationToken cancellationToken = default)
        {
            if (_fastExitFlagged)
            {
                return ListFilterCommand.Abort;
            }
            ArtifactInfo? info = await _artifactTool.RegistrationManager.TryGetArtifactAsync(artifactKey, cancellationToken).ConfigureAwait(false);
            if (info != null && info.Full)
            {
                _fastExitFlagged = true;
                return ListFilterCommand.Abort;
            }
            return ListFilterCommand.Accept;
        }
    }

    private ValueTask DumpAsListAsync(IArtifactListTool artifactTool, CancellationToken cancellationToken = default)
    {
        FilterDelegate<ArtifactKey, ListFilterCommand>? filterPredicate = null;
        switch (Options.SkipMode)
        {
            case ArtifactSkipMode.None:
                break;
            case ArtifactSkipMode.FastExit:
                filterPredicate = new DumpContext(artifactTool).FilterFastExitAsync;
                break;
            case ArtifactSkipMode.FastExitFull:
                filterPredicate = new DumpContext(artifactTool).FilterFastExitFullAsync;
                break;
            case ArtifactSkipMode.Known:
                filterPredicate = new DumpContext(artifactTool).FilterKnownAsync;
                break;
            case ArtifactSkipMode.KnownFull:
                filterPredicate = new DumpContext(artifactTool).FilterKnownFullAsync;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        IAsyncEnumerable<IArtifactData> enumerable = filterPredicate != null ? artifactTool.ListAsync(filterPredicate, cancellationToken) : artifactTool.ListAsync(cancellationToken);
        if ((Options.EagerFlags & artifactTool.AllowedEagerModes & EagerFlags.ArtifactList) != 0)
        {
            enumerable = enumerable.EagerAsync();
        }
        return (Options.EagerFlags & artifactTool.AllowedEagerModes & EagerFlags.ArtifactDump) != 0
            ? DumpAsListEagerAsync(artifactTool, enumerable, cancellationToken)
            : DumpAsListNonEagerAsync(artifactTool, enumerable, cancellationToken);
    }

    private async ValueTask DumpAsListEagerAsync(
        IArtifactListTool artifactTool,
        IAsyncEnumerable<IArtifactData> enumerable,
        CancellationToken cancellationToken = default)
    {
        List<Task> tasks = [];
        // CancellationToken is already used for the origin method call in DumpAsListAsync
        // ReSharper disable UseCancellationTokenForIAsyncEnumerable
        await foreach (IArtifactData data in enumerable.ConfigureAwait(false))
            // ReSharper restore UseCancellationTokenForIAsyncEnumerable
        {
            switch (Options.SkipMode)
            {
                case ArtifactSkipMode.None:
                    break;
                case ArtifactSkipMode.FastExit:
                    {
                        ArtifactInfo? info = await artifactTool.RegistrationManager.TryGetArtifactAsync(data.Info.Key, cancellationToken).ConfigureAwait(false);
                        if (info != null)
                        {
                            await WaitForTasksAsync(tasks).ConfigureAwait(false);
                            return;
                        }
                        break;
                    }
                case ArtifactSkipMode.FastExitFull:
                    {
                        ArtifactInfo? info = await artifactTool.RegistrationManager.TryGetArtifactAsync(data.Info.Key, cancellationToken).ConfigureAwait(false);
                        if (info != null && info.Full)
                        {
                            await WaitForTasksAsync(tasks).ConfigureAwait(false);
                            return;
                        }
                        break;
                    }
                case ArtifactSkipMode.Known:
                    {
                        ArtifactInfo? info = await artifactTool.RegistrationManager.TryGetArtifactAsync(data.Info.Key, cancellationToken).ConfigureAwait(false);
                        if (info != null)
                        {
                            continue;
                        }
                        break;
                    }
                case ArtifactSkipMode.KnownFull:
                    {
                        ArtifactInfo? info = await artifactTool.RegistrationManager.TryGetArtifactAsync(data.Info.Key, cancellationToken).ConfigureAwait(false);
                        if (info != null && info.Full)
                        {
                            continue;
                        }
                        break;
                    }
            }
            if (!data.Info.Full && !Options.IncludeNonFull)
            {
                continue;
            }
            tasks.Add(DumpArtifactAsync(data, artifactTool, cancellationToken));
        }
        await WaitForTasksAsync(tasks).ConfigureAwait(false);
    }

    private async ValueTask DumpAsListNonEagerAsync(
        IArtifactListTool artifactTool,
        IAsyncEnumerable<IArtifactData> enumerable,
        CancellationToken cancellationToken = default)
    {
        // CancellationToken is already used for the origin method call in DumpAsListAsync
        // ReSharper disable UseCancellationTokenForIAsyncEnumerable
        await foreach (IArtifactData data in enumerable.ConfigureAwait(false))
            // ReSharper restore UseCancellationTokenForIAsyncEnumerable
        {
            switch (Options.SkipMode)
            {
                case ArtifactSkipMode.None:
                    break;
                case ArtifactSkipMode.FastExit:
                    {
                        ArtifactInfo? info = await artifactTool.RegistrationManager.TryGetArtifactAsync(data.Info.Key, cancellationToken).ConfigureAwait(false);
                        if (info != null)
                        {
                            return;
                        }
                        break;
                    }
                case ArtifactSkipMode.FastExitFull:
                    {
                        ArtifactInfo? info = await artifactTool.RegistrationManager.TryGetArtifactAsync(data.Info.Key, cancellationToken).ConfigureAwait(false);
                        if (info != null && info.Full)
                        {
                            return;
                        }
                        break;
                    }
                case ArtifactSkipMode.Known:
                    {
                        ArtifactInfo? info = await artifactTool.RegistrationManager.TryGetArtifactAsync(data.Info.Key, cancellationToken).ConfigureAwait(false);
                        if (info != null)
                        {
                            continue;
                        }
                        break;
                    }
                case ArtifactSkipMode.KnownFull:
                    {
                        ArtifactInfo? info = await artifactTool.RegistrationManager.TryGetArtifactAsync(data.Info.Key, cancellationToken).ConfigureAwait(false);
                        if (info != null && info.Full)
                        {
                            continue;
                        }
                        break;
                    }
            }
            if (!data.Info.Full && !Options.IncludeNonFull)
            {
                continue;
            }
            try
            {
                await DumpArtifactAsync(data, artifactTool, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (Options.IgnoreException is { } ignoreException && ignoreException(e))
                {
                    LogHandler?.Log($"Ignored exception of type {e.GetType().FullName}", e.ToString(), LogLevel.Warning);
                }
                else
                {
                    throw;
                }
            }
        }
    }

    private Task DumpArtifactAsync(IArtifactData data, IArtifactListTool artifactTool, CancellationToken cancellationToken)
    {
        return artifactTool.DumpArtifactAsync(
            data,
            Options.ResourceUpdate,
            LogHandler,
            Options.ChecksumSource,
            artifactTool.TimeProvider,
            artifactTool.Config.GetArtifactRetrievalTimestamps,
            artifactTool.Config.GetResourceRetrievalTimestamps,
            Options.EagerFlags,
            cancellationToken);
    }

    private async Task WaitForTasksAsync(IReadOnlyList<Task> tasks)
    {
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
        {
            throw new AggregateException(exc);
        }
    }
}
