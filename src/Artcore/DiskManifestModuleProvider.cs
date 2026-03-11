using System.Diagnostics.CodeAnalysis;

namespace Artcore;

/// <summary>
/// Provides disk and manifest backed module provider.
/// </summary>
[RequiresUnreferencedCode("Loading modules might require types that cannot be statically analyzed.")]
public class DiskManifestModuleProvider : IModuleProvider<ModuleManifest, ALCModule>
{
    private readonly DiskManifestModuleLookup _lookup;
    private readonly DiskManifestModuleLoader _loader;

    /// <summary>
    /// Creates an instance of <see cref="DiskManifestModuleProvider"/>.
    /// </summary>
    /// <param name="moduleLoadConfiguration">Load configuration.</param>
    /// <param name="moduleDirectory">Module directory.</param>
    /// <param name="directorySuffix">Suffix on module directories.</param>
    /// <param name="fileNameSuffix">Suffix on module manifests.</param>
    /// <returns>Instance.</returns>
    public static DiskManifestModuleProvider Create(
        ModuleLoadConfiguration moduleLoadConfiguration,
        string moduleDirectory,
        string directorySuffix,
        string fileNameSuffix)
    {
        return new DiskManifestModuleProvider(
            DiskManifestModuleLookup.Create(moduleDirectory, directorySuffix, fileNameSuffix),
            DiskManifestModuleLoader.Create(moduleLoadConfiguration)
        );
    }

    private DiskManifestModuleProvider(DiskManifestModuleLookup lookup, DiskManifestModuleLoader loader)
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
    public ALCModule LoadModule(IModuleLocation moduleLocation)
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
