using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Loader;

namespace Artcore;

/// <summary>
/// Provides module search provider setup and parsing of module search files.
/// </summary>
public static class ModuleSearchConfigurationUtility
{
    /// <summary>
    /// Creates module providers corresponding to provided search config file paths.
    /// </summary>
    /// <param name="moduleLoadConfiguration">Configuration to use for loading modules.</param>
    /// <param name="baseDirectory">Base directory to apply search paths on.</param>
    /// <param name="paths">Paths to search config files.</param>
    /// <param name="moduleCreationFunc">Function to use for creating a module.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <typeparam name="TModule">Module type.</typeparam>
    /// <returns>Task returning list of module providers.</returns>
    [RequiresUnreferencedCode("Loading modules might require types that cannot be statically analyzed.")]
    public static async Task<List<IModuleProvider<TModule>>> GetModuleProvidersByPathsAsync<TModule>(
        ModuleLoadConfiguration moduleLoadConfiguration,
        string baseDirectory,
        IEnumerable<string> paths,
        Func<AssemblyLoadContext, Assembly, TModule> moduleCreationFunc,
        CancellationToken cancellationToken = default)
    {
        var configurations = new List<ModuleSearchConfiguration>();
        foreach (string path in paths)
        {
            configurations.Add(await ModuleSearchConfiguration.ParseFileAsync(path, cancellationToken));
        }
        return GetModuleProviders(moduleLoadConfiguration, baseDirectory, configurations, moduleCreationFunc);
    }

    /// <summary>
    /// Creates module providers corresponding to provided search config file paths.
    /// </summary>
    /// <param name="moduleLoadConfiguration">Configuration to use for loading modules.</param>
    /// <param name="baseDirectory">Base directory to apply search paths on.</param>
    /// <param name="paths">Paths to search config files.</param>
    /// <param name="moduleCreationFunc">Function to use for creating a module.</param>
    /// <typeparam name="TModule">Module type.</typeparam>
    /// <returns>List of module providers.</returns>
    [RequiresUnreferencedCode("Loading modules might require types that cannot be statically analyzed.")]
    public static List<IModuleProvider<TModule>> GetModuleProvidersByPaths<TModule>(
        ModuleLoadConfiguration moduleLoadConfiguration,
        string baseDirectory,
        IEnumerable<string> paths,
        Func<AssemblyLoadContext, Assembly, TModule> moduleCreationFunc)
    {
        var configurations = new List<ModuleSearchConfiguration>();
        foreach (string path in paths)
        {
            configurations.Add(ModuleSearchConfiguration.ParseFile(path));
        }
        return GetModuleProviders(moduleLoadConfiguration, baseDirectory, configurations, moduleCreationFunc);
    }

    /// <summary>
    /// Creates module providers corresponding to provided search configs.
    /// </summary>
    /// <param name="moduleLoadConfiguration">Configuration to use for loading modules.</param>
    /// <param name="baseDirectory">Base directory to apply search paths on.</param>
    /// <param name="searchConfigurations">Search configurations.</param>
    /// <param name="moduleCreationFunc">Function to use for creating a module.</param>
    /// <typeparam name="TModule">Module type.</typeparam>
    /// <returns>List of module providers.</returns>
    [RequiresUnreferencedCode("Loading modules might require types that cannot be statically analyzed.")]
    public static List<IModuleProvider<TModule>> GetModuleProviders<TModule>(
        ModuleLoadConfiguration moduleLoadConfiguration,
        string baseDirectory,
        IEnumerable<ModuleSearchConfiguration> searchConfigurations,
        Func<AssemblyLoadContext, Assembly, TModule> moduleCreationFunc)
    {
        var providers = new List<IModuleProvider<TModule>>();
        foreach (var searchConfiguration in searchConfigurations)
        {
            foreach (var entry in searchConfiguration.Entries)
            {
                providers.Add(ModuleManifestProvider<TModule>.Create(
                    moduleLoadConfiguration,
                    Path.Combine(baseDirectory, entry.Path),
                    entry.DirectorySuffix,
                    entry.FileNameSuffix,
                    moduleCreationFunc));
            }
        }
        return providers;
    }

}
