﻿namespace Art.Common;

/// <summary>
/// Extensions for <see cref="ArtifactInfo"/>.
/// </summary>
public static class ArtifactInfoExtensions
{
    /// <summary>
    /// Gets informational title string.
    /// </summary>
    /// <returns>Info title string.</returns>
    public static string GetInfoTitleString(this ArtifactInfo artifactInfo) => $"{(!artifactInfo.Full ? "[partial] " : "")}{artifactInfo.Key.Id}{(artifactInfo.Name != null ? $" - {artifactInfo.Name}" : "")}";

    /// <summary>
    /// Gets informational string.
    /// </summary>
    /// <returns>Info string.</returns>
    public static string GetInfoString(this ArtifactInfo artifactInfo) => $"ID: {artifactInfo.Key.Id}{(artifactInfo.Name != null ? $"\nName: {artifactInfo.Name}" : "")}{(artifactInfo.Date != null ? $"\nDate: {artifactInfo.Date}" : "")}{(artifactInfo.UpdateDate != null ? $"\nUpdate Date: {artifactInfo.UpdateDate}" : "")}{(artifactInfo.RetrievalDate != null ? $"\nRetrieval Date: {artifactInfo.RetrievalDate}" : "")}\nFull: {artifactInfo.Full}";
}
