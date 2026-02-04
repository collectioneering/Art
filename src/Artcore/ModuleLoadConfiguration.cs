using System.Collections.Immutable;
using System.Runtime.Loader;

namespace Artcore;

/// <summary>
/// Represents a configuration for loading on a <see cref="ModuleManifestProvider{TModule}"/>.
/// </summary>
/// <param name="PassthroughAssemblies">Assemblies to pass through to default <see cref="AssemblyLoadContext"/>.</param>
public record ModuleLoadConfiguration(ImmutableHashSet<string> PassthroughAssemblies)
{
    /// <summary>
    /// Creates an instance of <see cref="ModuleLoadConfiguration"/>.
    /// </summary>
    /// <param name="passthroughAssemblies">Assemblies to pass through to default <see cref="AssemblyLoadContext"/>.</param>
    public static ModuleLoadConfiguration Create(params string[] passthroughAssemblies)
    {
        return new ModuleLoadConfiguration(passthroughAssemblies.ToImmutableHashSet());
    }
}
