using System.Diagnostics.CodeAnalysis;

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
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Task returning list of module providers.</returns>
    [RequiresUnreferencedCode("Loading modules might require types that cannot be statically analyzed.")]
    public static async Task<List<IModuleProvider<ALCModule>>> GetModuleProvidersByPathsAsync(
        ModuleLoadConfiguration moduleLoadConfiguration,
        string baseDirectory,
        IEnumerable<string> paths,
        CancellationToken cancellationToken = default)
    {
        var configurations = new List<ModuleSearchConfiguration>();
        foreach (string path in paths)
        {
            configurations.Add(await ModuleSearchConfiguration.ParseFileAsync(path, cancellationToken));
        }
        return GetModuleProviders(moduleLoadConfiguration, baseDirectory, configurations);
    }

    /// <summary>
    /// Creates module providers corresponding to provided search config file paths.
    /// </summary>
    /// <param name="moduleLoadConfiguration">Configuration to use for loading modules.</param>
    /// <param name="baseDirectory">Base directory to apply search paths on.</param>
    /// <param name="paths">Paths to search config files.</param>
    /// <returns>List of module providers.</returns>
    [RequiresUnreferencedCode("Loading modules might require types that cannot be statically analyzed.")]
    public static List<IModuleProvider<ALCModule>> GetModuleProvidersByPaths(
        ModuleLoadConfiguration moduleLoadConfiguration,
        string baseDirectory,
        IEnumerable<string> paths)
    {
        var configurations = new List<ModuleSearchConfiguration>();
        foreach (string path in paths)
        {
            configurations.Add(ModuleSearchConfiguration.ParseFile(path));
        }
        return GetModuleProviders(moduleLoadConfiguration, baseDirectory, configurations);
    }

    /// <summary>
    /// Creates module providers corresponding to provided search configs.
    /// </summary>
    /// <param name="moduleLoadConfiguration">Configuration to use for loading modules.</param>
    /// <param name="baseDirectory">Base directory to apply search paths on.</param>
    /// <param name="searchConfigurations">Search configurations.</param>
    /// <returns>List of module providers.</returns>
    [RequiresUnreferencedCode("Loading modules might require types that cannot be statically analyzed.")]
    public static List<IModuleProvider<ALCModule>> GetModuleProviders(
        ModuleLoadConfiguration moduleLoadConfiguration,
        string baseDirectory,
        IEnumerable<ModuleSearchConfiguration> searchConfigurations)
    {
        var providers = new List<IModuleProvider<ALCModule>>();
        foreach (var searchConfiguration in searchConfigurations)
        {
            foreach (var entry in searchConfiguration.Entries)
            {
                providers.Add(DiskManifestModuleProvider.Create(
                    moduleLoadConfiguration,
                    Path.Combine(baseDirectory, entry.Path),
                    entry.DirectorySuffix,
                    entry.FileNameSuffix));
            }
        }
        return providers;
    }

}
