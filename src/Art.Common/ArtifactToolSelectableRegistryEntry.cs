using System.Diagnostics.CodeAnalysis;

namespace Art.Common;

/// <summary>
/// Represents an entry for a <see cref="IArtifactToolRegistry"/>.
/// </summary>
/// <param name="Id">Tool name.</param>
public abstract record ArtifactToolSelectableRegistryEntry(ArtifactToolID Id) : ArtifactToolRegistryEntry(Id)
{
    /// <summary>
    /// Attempts to identify an applicable tool ID and artifact ID from an input key.
    /// </summary>
    /// <param name="key">Key to evaluate.</param>
    /// <param name="artifactToolId">Artifact tool ID, if successful.</param>
    /// <param name="artifactId">Artifact ID, if successful.</param>
    /// <returns>True if successful.</returns>
    public abstract bool TryIdentify(string key, [NotNullWhen(true)] out ArtifactToolID? artifactToolId, [NotNullWhen(true)] out string? artifactId);
}

/// <summary>
/// Represents an entry for a <see cref="IArtifactToolRegistry"/>.
/// </summary>
/// <param name="Id">Tool name.</param>
/// <typeparam name="T">Tool type.</typeparam>
public record ArtifactToolSelectableRegistryEntry<T>(ArtifactToolID Id) : ArtifactToolSelectableRegistryEntry(Id) where T : IArtifactToolFactory, IArtifactToolSelector<string>
{
    /// <inheritdoc />
    public override IArtifactTool CreateArtifactTool()
    {
        return T.CreateArtifactTool();
    }

    /// <inheritdoc />
    public override Type GetArtifactToolType()
    {
        return T.GetArtifactToolType();
    }

    /// <inheritdoc />
    public override bool TryIdentify(string key, [NotNullWhen(true)] out ArtifactToolID? artifactToolId, [NotNullWhen(true)] out string? artifactId)
    {
        if (T.TryIdentify(key, out _, out artifactId))
        {
            artifactToolId = Id;
            return true;
        }
        artifactToolId = null;
        return false;
    }
}
