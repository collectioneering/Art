using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Art.Common;
using Art.Common.Modular;

namespace Art.Tesler;

public static class ModuleSearchConfigurationUtility
{
    public static async Task<ModuleSearchConfiguration?> ParseConfigFileAsync(string configFile, CancellationToken cancellationToken = default)
    {
        await using var fileStream = File.OpenRead(configFile);
        return await JsonSerializer.DeserializeAsync(fileStream, SourceGenerationContext.Default.ModuleSearchConfiguration, cancellationToken);
    }


    public static ModuleSearchConfiguration? ParseConfigFile(string configFile)
    {
        using var fileStream = File.OpenRead(configFile);
        return JsonSerializer.Deserialize(fileStream, SourceGenerationContext.Default.ModuleSearchConfiguration);
    }

    [RequiresUnreferencedCode("Loading artifact tools might require types that cannot be statically analyzed.")]
    public static async Task<List<IModuleProvider>> GetModuleProvidersByPathsAsync(
        ModuleLoadConfiguration moduleLoadConfiguration,
        string baseDirectory,
        IEnumerable<string> paths,
        CancellationToken cancellationToken = default)
    {
        var configurations = new List<ModuleSearchConfiguration>();
        foreach (string path in paths)
        {
            if (await ParseConfigFileAsync(path, cancellationToken) is { } result)
            {
                configurations.Add(result);
            }
        }
        return GetModuleProviders(moduleLoadConfiguration, baseDirectory, configurations);
    }

    [RequiresUnreferencedCode("Loading artifact tools might require types that cannot be statically analyzed.")]
    public static List<IModuleProvider> GetModuleProvidersByPaths(
        ModuleLoadConfiguration moduleLoadConfiguration,
        string baseDirectory,
        IEnumerable<string> paths)
    {
        var configurations = new List<ModuleSearchConfiguration>();
        foreach (string path in paths)
        {
            if (ParseConfigFile(path) is { } result)
            {
                configurations.Add(result);
            }
        }
        return GetModuleProviders(moduleLoadConfiguration, baseDirectory, configurations);
    }

    [RequiresUnreferencedCode("Loading artifact tools might require types that cannot be statically analyzed.")]
    public static List<IModuleProvider> GetModuleProviders(
        ModuleLoadConfiguration moduleLoadConfiguration,
        string baseDirectory,
        IEnumerable<ModuleSearchConfiguration> searchConfigurations)
    {
        var providers = new List<IModuleProvider>();
        foreach (var searchConfiguration in searchConfigurations)
        {
            foreach (var entry in searchConfiguration.Entries)
            {
                providers.Add(ModuleManifestProvider.Create(
                    moduleLoadConfiguration,
                    Path.Combine(baseDirectory, entry.Path),
                    entry.DirectorySuffix,
                    entry.FileNameSuffix));
            }
        }
        return providers;
    }

    public record ModuleSearchConfiguration(
        [property: JsonPropertyName("Entries")] ModuleSearchConfigurationEntry[] Entries);

    public record ModuleSearchConfigurationEntry(
        [property: JsonPropertyName("Path")] string Path,
        [property: JsonPropertyName("Name")] string? Name,
        [property: JsonPropertyName("DirectorySuffix")] string DirectorySuffix,
        [property: JsonPropertyName("FileNameSuffix")] string FileNameSuffix);
}
