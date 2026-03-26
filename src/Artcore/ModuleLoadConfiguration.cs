using System.Collections.Immutable;
using System.Runtime.Loader;

namespace Artcore;

/// <summary>
/// Represents a configuration for loading on a <see cref="DiskManifestModuleProvider"/>.
/// </summary>
/// <param name="IsCollectible">If true, allow usage of <see cref="AssemblyLoadContext.Unload"/>.</param>
/// <param name="PassthroughAssemblies">Assemblies to pass through to default <see cref="AssemblyLoadContext"/>.</param>
public record ModuleLoadConfiguration(bool IsCollectible, ImmutableHashSet<string> PassthroughAssemblies)
{
    /// <summary>
    /// Creates an instance of <see cref="ModuleLoadConfiguration"/>.
    /// </summary>
    /// <param name="isCollectible">If true, allow usage of <see cref="AssemblyLoadContext.Unload"/>.</param>
    /// <param name="passthroughAssemblies">Assemblies to pass through to default <see cref="AssemblyLoadContext"/>.</param>
    public static ModuleLoadConfiguration Create(bool isCollectible, params string[] passthroughAssemblies)
    {
        return new ModuleLoadConfiguration(isCollectible, passthroughAssemblies.ToImmutableHashSet());
    }
}
