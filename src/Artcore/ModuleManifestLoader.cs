using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Artcore;

/// <summary>
/// Provides loading of modules stored on disks with manifests.
/// </summary>
[RequiresUnreferencedCode("Loading modules might require types that cannot be statically analyzed.")]
public class ModuleManifestLoader : IModuleLoader<ALCModule>
{
    private readonly ModuleLoadConfiguration _moduleLoadConfiguration;

    /// <summary>
    /// Creates an instance of <see cref="ModuleManifestLoader"/>.
    /// </summary>
    /// <param name="moduleLoadConfiguration">Load configuration.</param>
    /// <returns>Instance.</returns>
    public static ModuleManifestLoader Create(ModuleLoadConfiguration moduleLoadConfiguration)
    {
        return new ModuleManifestLoader(moduleLoadConfiguration);
    }

    private ModuleManifestLoader( ModuleLoadConfiguration moduleLoadConfiguration)
    {
        _moduleLoadConfiguration = moduleLoadConfiguration;
    }

    /// <inheritdoc />
    public bool CanLoadModule(IModuleLocation moduleLocation)
    {
        return moduleLocation is ModuleManifest;
    }

    /// <inheritdoc />
    public ALCModule LoadModule(IModuleLocation moduleLocation)
    {
        if (moduleLocation is not ModuleManifest manifest)
        {
            throw new ArgumentException("Cannot load this module manifest, it is of an invalid type.");
        }
        string baseDir = manifest.Content.Path != null ? Path.Combine(manifest.BasePath, manifest.Content.Path) : manifest.BasePath;
        var ctx = new RestrictedPassthroughAssemblyLoadContext(baseDir, manifest.Content.Assembly, _moduleLoadConfiguration.PassthroughAssemblies);
        return new ALCModule(ctx.LoadFromAssemblyName(new AssemblyName(manifest.Content.Assembly)), ctx);
    }
}
