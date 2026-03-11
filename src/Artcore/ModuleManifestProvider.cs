using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Loader;

namespace Artcore;

/// <summary>
/// Provides disk and manifest backed module provider.
/// </summary>
[RequiresUnreferencedCode("Loading modules might require types that cannot be statically analyzed.")]
public class ModuleManifestProvider<TModule> : IModuleProvider<ModuleManifest, TModule>
{
    private readonly ModuleManifestLookup _lookup;
    private readonly ModuleManifestLoader<TModule> _loader;

    /// <summary>
    /// Creates an instance of <see cref="ModuleManifestProvider{TModule}"/>.
    /// </summary>
    /// <param name="moduleLoadConfiguration">Load configuration.</param>
    /// <param name="moduleDirectory">Module directory.</param>
    /// <param name="directorySuffix">Suffix on module directories.</param>
    /// <param name="fileNameSuffix">Suffix on module manifests.</param>
    /// <param name="creationFunction">Creation function.</param>
    /// <returns>Instance.</returns>
    public static ModuleManifestProvider<TModule> Create(
        ModuleLoadConfiguration moduleLoadConfiguration,
        string moduleDirectory,
        string directorySuffix,
        string fileNameSuffix,
        Func<AssemblyLoadContext, Assembly, TModule> creationFunction)
    {
        return new ModuleManifestProvider<TModule>(
            ModuleManifestLookup.Create(moduleDirectory, directorySuffix, fileNameSuffix),
            ModuleManifestLoader<TModule>.Create(moduleLoadConfiguration, creationFunction)
        );
    }

    private ModuleManifestProvider(ModuleManifestLookup lookup, ModuleManifestLoader<TModule> loader)
    {
        _lookup = lookup;
        _loader = loader;
    }

    /// <inheritdoc />
    public bool CanLoadModule(IModuleLocation moduleLocation)
    {
        return _loader.CanLoadModule(moduleLocation);
    }

    /// <inheritdoc />
    public TModule LoadModule(IModuleLocation moduleLocation)
    {
        return _loader.LoadModule(moduleLocation);
    }

    /// <inheritdoc />
    public bool TryLocateModule(string assembly, [NotNullWhen(true)] out ModuleManifest? moduleLocation)
    {
        return _lookup.TryLocateModule(assembly, out moduleLocation);
    }

    /// <inheritdoc />
    public IEnumerable<IModuleLocation> LoadModuleLocations()
    {
        return _lookup.LoadModuleLocations();
    }

    /// <inheritdoc />
    public IEnumerable<ModuleManifest> LoadTypedModuleLocations()
    {
        return _lookup.LoadTypedModuleLocations();
    }
}
