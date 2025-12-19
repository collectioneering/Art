using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Art.Common;

/// <summary>
/// Represents a regex-based provider to identify an applicable tool ID and artifact ID.
/// </summary>
public interface IArtifactToolRegexSelector<TSelf> : IArtifactToolFactory, IArtifactToolSelector<string> where TSelf : IArtifactToolRegexSelector<TSelf>
{
    /// <summary>
    /// Gets artifact tool selector regex.
    /// </summary>
    /// <returns>Regex to identify tools with.</returns>
    static abstract Regex GetArtifactToolSelectorRegex();

    /// <summary>
    /// Gets group name for the regex returned by <see cref="IArtifactToolRegexSelector{TSelf}.GetArtifactToolSelectorRegex"/> that corresponds to the artifact ID.
    /// </summary>
    /// <returns>Group name.</returns>
    static abstract string GetArtifactToolSelectorRegexIdGroupName();

    static bool IArtifactToolSelector<string>.TryIdentify(string key, [NotNullWhen(true)] out ArtifactToolID? artifactToolId, [NotNullWhen(true)] out string? artifactId)
    {
        if (TSelf.GetArtifactToolSelectorRegex().Match(key) is { Success: true } match
            && match.Groups.TryGetValue(TSelf.GetArtifactToolSelectorRegexIdGroupName(), out Group? group)
            && group.Success)
        {
            artifactToolId = TSelf.GetArtifactToolId();
            artifactId = group.Value;
            return true;
        }
        artifactToolId = null;
        artifactId = null;
        return false;
    }
}
