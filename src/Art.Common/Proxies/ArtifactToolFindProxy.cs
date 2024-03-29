﻿namespace Art.Common.Proxies;

/// <summary>
/// Proxy to run artifact tool as a find tool.
/// </summary>
public record ArtifactToolFindProxy
{
    /// <summary>
    /// Proxy to run artifact tool as a find tool.
    /// </summary>
    /// <param name="artifactTool">Artifact tool.</param>
    /// <param name="logHandler">Log handler.</param>
    public ArtifactToolFindProxy(IArtifactTool artifactTool, IToolLogHandler? logHandler)
    {
        if (artifactTool == null) throw new ArgumentNullException(nameof(artifactTool));
        ArtifactTool = artifactTool;
        LogHandler = logHandler;
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
        if (LogHandler != null) artifactTool.LogHandler = LogHandler;
        if (artifactTool is IArtifactFindTool findTool)
            return await findTool.FindAsync(id, cancellationToken).ConfigureAwait(false);
        throw new NotSupportedException("Artifact tool is not a supported type");
    }

    #endregion

    /// <summary>Artifact tool.</summary>
    public IArtifactTool ArtifactTool { get; init; }

    /// <summary>Log handler.</summary>
    public IToolLogHandler? LogHandler { get; init; }
}
