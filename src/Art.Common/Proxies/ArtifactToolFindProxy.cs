namespace Art.Common.Proxies;

/// <summary>
/// Proxy to run artifact tool as a find tool.
/// </summary>
public record ArtifactToolFindProxy
{
    /// <summary>Artifact tool.</summary>
    public IArtifactTool ArtifactTool { get; init; }

    /// <summary>Log handler.</summary>
    public IToolLogHandler? LogHandler { get; init; }

    /// <summary>Preferences to use when logging.</summary>
    public LogPreferences LogPreferences { get; init; }

    /// <summary>
    /// Proxy to run artifact tool as a find tool.
    /// </summary>
    /// <param name="artifactTool">Artifact tool.</param>
    /// <param name="logHandler">Log handler.</param>
    /// <param name="logPreferences">Preferences to use when logging.</param>
    public ArtifactToolFindProxy(
        IArtifactTool artifactTool,
        IToolLogHandler? logHandler,
        LogPreferences logPreferences)
    {
        ArtifactTool = artifactTool ?? throw new ArgumentNullException(nameof(artifactTool));
        LogHandler = logHandler;
        LogPreferences = logPreferences;
    }

    #region API

    /// <summary>
    /// Finds artifacts.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async-enumerable artifacts.</returns>
    public async Task<IArtifactData?> FindAsync(string id, CancellationToken cancellationToken = default)
    {
        if (ArtifactTool == null) throw new InvalidOperationException("Artifact tool cannot be null");
        IArtifactTool artifactTool = ArtifactTool;
        var existingLogHandler = artifactTool.LogHandler;
        var existingLogPreferences = artifactTool.LogPreferences;
        try
        {
            if (LogHandler != null)
            {
                artifactTool.LogHandler = LogHandler;
            }
            artifactTool.LogPreferences = LogPreferences;
            if (artifactTool is IArtifactFindTool findTool)
            {
                return await findTool.FindAsync(id, cancellationToken).ConfigureAwait(false);
            }
            throw new NotSupportedException("Artifact tool is not a supported type");
        }
        finally
        {
            artifactTool.LogHandler = existingLogHandler;
            artifactTool.LogPreferences = existingLogPreferences;
        }
    }

    #endregion
}
