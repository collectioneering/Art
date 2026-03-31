using System.Diagnostics.CodeAnalysis;

namespace Art.Common;

/// <summary>
/// Represents a registry of artifact tools composed of multiple <see cref="IArtifactToolRegistry"/>.
/// </summary>
/// <remarks>
/// Contains, TryGetType, TryLoad, TryIdentify, GetToolDescriptions uses FIFO order for registries.
/// </remarks>
public class AggregateArtifactToolRegistry : IArtifactToolSelectableRegistry<string>
{
    private readonly IArtifactToolRegistry[] _artifactToolRegistries;

    /// <summary>
    /// Initializes an instance of <see cref="StaticArtifactToolRegistryStore"/>.
    /// </summary>
    /// <param name="artifactToolRegistries">Registries to use, with earlier entries having precedence.</param>
    public AggregateArtifactToolRegistry(IReadOnlyList<IArtifactToolRegistry> artifactToolRegistries)
    {
        _artifactToolRegistries = artifactToolRegistries.ToArray();
    }

    /// <inheritdoc />
    /// <remarks>
    /// This method uses FIFO order for the underlying registries.
    /// </remarks>
    public bool Contains(ArtifactToolID artifactToolId)
    {
        foreach (var registry in _artifactToolRegistries)
        {
            if (registry.Contains(artifactToolId))
            {
                return true;
            }
        }
        return false;
    }

    /// <inheritdoc />
    /// <remarks>
    /// This method uses FIFO order for the underlying registries.
    /// </remarks>
    public bool TryGetType(ArtifactToolID artifactToolId, [NotNullWhen(true)] out Type? type)
    {
        foreach (var registry in _artifactToolRegistries)
        {
            if (registry.TryGetType(artifactToolId, out type))
            {
                return true;
            }
        }
        type = null;
        return false;
    }

    /// <inheritdoc />
    /// <remarks>
    /// This method uses FIFO order for the underlying registries.
    /// </remarks>
    public bool TryLoad(ArtifactToolID artifactToolId, [NotNullWhen(true)] out IArtifactTool? tool)
    {
        foreach (var registry in _artifactToolRegistries)
        {
            if (registry.TryLoad(artifactToolId, out tool))
            {
                return true;
            }
        }
        tool = null;
        return false;
    }

    /// <inheritdoc />
    /// <remarks>
    /// This method uses FIFO order for the underlying registries.
    /// </remarks>
    public IEnumerable<ArtifactToolDescription> GetToolDescriptions()
    {
        HashSet<ArtifactToolID> known = [];
        foreach (var registry in _artifactToolRegistries)
        {
            foreach (var description in registry.GetToolDescriptions())
            {
                if (known.Add(description.Id))
                {
                    yield return description;
                }
            }
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// This method uses FIFO order for the underlying registries.
    /// </remarks>
    public bool TryIdentify(string key, [NotNullWhen(true)] out ArtifactToolID? artifactToolId, [NotNullWhen(true)] out string? artifactId)
    {
        foreach (var registry in _artifactToolRegistries)
        {
            if (registry is IArtifactToolSelectableRegistry<string> selectorRegistry && selectorRegistry.TryIdentify(key, out artifactToolId, out artifactId))
            {
                return true;
            }
        }
        artifactToolId = null;
        artifactId = null;
        return false;
    }
}
