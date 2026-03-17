using System.Diagnostics.CodeAnalysis;

namespace Art.Common;

/// <summary>
/// Represents a store of <see cref="IArtifactToolRegistry"/> composed of multiple <see cref="IArtifactToolRegistryStore"/>.
/// </summary>
/// <remarks>
/// TryLoadRegistry and LoadAllRegistries use FIFO order for registries.
/// </remarks>
public class AggregateArtifactToolRegistryStore : IArtifactToolRegistryStore
{
    private readonly IArtifactToolRegistryStore[] _artifactToolRegistryStores;

    /// <summary>
    /// Initializes an instance of <see cref="AggregateArtifactToolRegistryStore"/>.
    /// </summary>
    /// <param name="artifactToolRegistryStores">Registry stores to use, with earlier entries having precedence.</param>
    public AggregateArtifactToolRegistryStore(IReadOnlyList<IArtifactToolRegistryStore> artifactToolRegistryStores)
    {
        _artifactToolRegistryStores = artifactToolRegistryStores.ToArray();
    }

    /// <inheritdoc />
    /// <remarks>
    /// This method uses FIFO order for the underlying registry stores.
    /// </remarks>
    public bool TryLoadRegistry(ArtifactToolID artifactToolId, [NotNullWhen(true)] out IArtifactToolRegistry? artifactToolRegistry)
    {
        foreach (var artifactToolRegistryStore in _artifactToolRegistryStores)
        {
            if (artifactToolRegistryStore.TryLoadRegistry(artifactToolId, out artifactToolRegistry))
            {
                return true;
            }
        }
        artifactToolRegistry = null;
        return false;
    }

    /// <inheritdoc />
    /// <remarks>
    /// This method uses FIFO order for the underlying registry stores.
    /// </remarks>
    public IEnumerable<IArtifactToolRegistry> LoadAllRegistries()
    {
        foreach (var artifactToolRegistryStore in _artifactToolRegistryStores)
        {
            foreach (var artifactToolRegistry in artifactToolRegistryStore.LoadAllRegistries())
            {
                yield return artifactToolRegistry;
            }
        }
    }
}
