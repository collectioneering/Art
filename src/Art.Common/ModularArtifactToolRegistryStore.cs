using System.Diagnostics.CodeAnalysis;
using Artcore;

namespace Art.Common;

/// <summary>
/// Represents a store of dynamically loaded <see cref="IArtifactToolRegistry"/> based on a <see cref="IModuleProvider{TModule}"/>.
/// </summary>
[RequiresUnreferencedCode("Loading artifact tools might require types that cannot be statically analyzed.")]
public class ModularArtifactToolRegistryStore : IArtifactToolRegistryStore
{
    private readonly IModuleProvider<ALCModule> _moduleProvider;

    /// <summary>
    /// Initializes an instance of <see cref="ModularArtifactToolRegistryStore"/>.
    /// </summary>
    /// <param name="moduleProvider"><see cref="IModuleProvider{TModule}"/>.</param>
    public ModularArtifactToolRegistryStore(IModuleProvider<ALCModule> moduleProvider)
    {
        _moduleProvider = moduleProvider;
    }

    /// <inheritdoc />
    public bool TryLoadRegistry(ArtifactToolID artifactToolId, [NotNullWhen(true)] out IArtifactToolRegistry? artifactToolRegistry)
    {
        string assembly = artifactToolId.Assembly;
        if (!_moduleProvider.TryLocateModule(assembly, out var module))
        {
            artifactToolRegistry = null;
            return false;
        }
        artifactToolRegistry = Plugin.Create(_moduleProvider.LoadModule(module));
        return true;
    }

    /// <inheritdoc />
    public IEnumerable<IArtifactToolRegistry> LoadAllRegistries()
    {
        foreach (var module in _moduleProvider.LoadModuleLocations())
        {
            yield return Plugin.Create(_moduleProvider.LoadModule(module));
        }
    }
}
