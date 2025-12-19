using System.Diagnostics.CodeAnalysis;

namespace Art.Common;

/// <summary>
/// Represents an aggregate registry that tries registries in LIFO order.
/// </summary>
public class AggregateArtifactToolRegistry : IArtifactToolSelectableRegistry<string>
{
    /// <summary>
    /// Contained registries.
    /// </summary>
    public IReadOnlyList<IArtifactToolRegistry> Registries => _registries;

    private readonly List<IArtifactToolRegistry> _registries = new();

    /// <summary>
    /// Adds a registry.
    /// </summary>
    /// <param name="registry">Registry to add.</param>
    /// <exception cref="ArgumentException">Thrown if registry was already added.</exception>
    public void Add(IArtifactToolRegistry registry)
    {
        if (registry == null)
        {
            throw new ArgumentNullException(nameof(registry));
        }
        if (_registries.Contains(registry))
        {
            throw new ArgumentException("Cannot add existing registry. It must first be removed.");
        }
        _registries.Add(registry);
    }

    /// <summary>
    /// Attempts to add a registry.
    /// </summary>
    /// <param name="registry">Registry to add.</param>
    /// <returns>True if registry was added.</returns>
    /// <remarks>This method can return false if the registry was already added - entries must be removed before they are added again.</remarks>
    public bool TryAdd(IArtifactToolRegistry registry)
    {
        // BCL collections can throw for null, do likewise on these
        if (registry == null)
        {
            throw new ArgumentNullException(nameof(registry));
        }
        if (_registries.Contains(registry))
        {
            return false;
        }
        _registries.Add(registry);
        return true;
    }

    /// <summary>
    /// Checks if registry is contained in this registry.
    /// </summary>
    /// <param name="registry">Registry.</param>
    /// <returns>True if contained.</returns>
    public bool Contains(ArtifactToolRegistry registry)
    {
        return _registries.Contains(registry);
    }

    /// <summary>
    /// Attempts to remove a registry.
    /// </summary>
    /// <param name="registry">Registry to remove.</param>
    /// <returns>True if successfully removed.</returns>
    public bool Remove(IArtifactToolRegistry registry)
    {
        return _registries.Remove(registry);
    }

    /// <summary>
    /// Removes all contained registries.
    /// </summary>
    public void Clear()
    {
        _registries.Clear();
    }

    /// <inheritdoc />
    public bool Contains(ArtifactToolID artifactToolId)
    {
        foreach (var registry in _registries)
        {
            if (registry.Contains(artifactToolId))
            {
                return true;
            }
        }
        return false;
    }

    /// <inheritdoc />
    public bool TryGetType(ArtifactToolID artifactToolId, [NotNullWhen(true)] out Type? type)
    {
        for (int i = _registries.Count - 1; i >= 0; i--)
        {
            if (_registries[i].TryGetType(artifactToolId, out type))
            {
                return true;
            }
        }
        type = null;
        return false;
    }

    /// <inheritdoc />
    public bool TryLoad(ArtifactToolID artifactToolId, [NotNullWhen(true)] out IArtifactTool? tool)
    {
        for (int i = _registries.Count - 1; i >= 0; i--)
        {
            if (_registries[i].TryLoad(artifactToolId, out tool))
            {
                return true;
            }
        }
        tool = null;
        return false;
    }

    /// <inheritdoc />
    public IEnumerable<ArtifactToolDescription> GetToolDescriptions()
    {
        HashSet<ArtifactToolID> known = new();
        foreach (var registry in _registries)
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
    public bool TryIdentify(string key, [NotNullWhen(true)] out ArtifactToolID? artifactToolId, [NotNullWhen(true)] out string? artifactId)
    {
        for (int i = _registries.Count - 1; i >= 0; i--)
        {
            if (_registries[i] is IArtifactToolSelectableRegistry<string> selectorRegistry && selectorRegistry.TryIdentify(key, out artifactToolId, out artifactId))
            {
                return true;
            }
        }
        artifactToolId = null;
        artifactId = null;
        return false;
    }
}
