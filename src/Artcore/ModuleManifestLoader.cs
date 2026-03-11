using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Loader;

namespace Artcore;

/// <summary>
/// Provides loading of modules stored on disks with manifests.
/// </summary>
[RequiresUnreferencedCode("Loading modules might require types that cannot be statically analyzed.")]
public class ModuleManifestLoader<TModule> : IModuleLoader<TModule>
{
    private readonly ModuleLoadConfiguration _moduleLoadConfiguration;
    private readonly Func<AssemblyLoadContext, Assembly, TModule> _creationFunction;

    /// <summary>
    /// Creates an instance of <see cref="ModuleManifestLoader{TModule}"/>.
    /// </summary>
    /// <param name="moduleLoadConfiguration">Load configuration.</param>
    /// <param name="creationFunction">Creation function.</param>
    /// <returns>Instance.</returns>
    public static ModuleManifestLoader<TModule> Create(
        ModuleLoadConfiguration moduleLoadConfiguration,
        Func<AssemblyLoadContext, Assembly, TModule> creationFunction)
    {
        return new ModuleManifestLoader<TModule>(moduleLoadConfiguration, creationFunction);
    }

    private ModuleManifestLoader(
        ModuleLoadConfiguration moduleLoadConfiguration,
        Func<AssemblyLoadContext, Assembly, TModule> creationFunction)
    {
        _moduleLoadConfiguration = moduleLoadConfiguration;
        _creationFunction = creationFunction;
    }

    /// <inheritdoc />
    public TModule LoadModule(IModuleLocation moduleLocation)
    {
        if (moduleLocation is not ModuleManifest manifest)
        {
            throw new ArgumentException("Cannot load this module manifest, it is of an invalid type.");
        }
        string baseDir = manifest.Content.Path != null && !Path.IsPathFullyQualified(manifest.Content.Path) ? Path.Combine(manifest.BasePath, manifest.Content.Path) : manifest.BasePath;
        var ctx = new RestrictedPassthroughAssemblyLoadContext(baseDir, manifest.Content.Assembly, _moduleLoadConfiguration.PassthroughAssemblies);
        return _creationFunction(ctx, ctx.LoadFromAssemblyName(new AssemblyName(manifest.Content.Assembly)));
    }
}
