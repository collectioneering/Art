using System.Text.Json;
using System.Text.Json.Serialization;

namespace Artcore;

/// <summary>
/// Represents a module search configuration.
/// </summary>
/// <param name="Entries">Configuration entries.</param>
public record ModuleSearchConfiguration(
    [property: JsonPropertyName("Entries")] ModuleSearchConfigurationEntry[] Entries)
{
    /// <summary>
    /// Parses a module search config file.
    /// </summary>
    /// <param name="configFile">Config file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning search configuration.</returns>
    public static async Task<ModuleSearchConfiguration> ParseFileAsync(string configFile, CancellationToken cancellationToken = default)
    {
        await using var fileStream = File.OpenRead(configFile);
        return await ParseUtf8StreamAsync(fileStream, cancellationToken);
    }

    /// <summary>
    /// Parses a module search config file.
    /// </summary>
    /// <param name="configFile">Config file.</param>
    /// <returns>Search configuration.</returns>
    public static ModuleSearchConfiguration ParseFile(string configFile)
    {
        using var fileStream = File.OpenRead(configFile);
        return ParseUtf8Stream(fileStream);
    }

    /// <summary>
    /// Parses a module search config file from a stream.
    /// </summary>
    /// <param name="stream">Stream of UTF-8 encoded JSON.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning search configuration.</returns>
    public static async Task<ModuleSearchConfiguration> ParseUtf8StreamAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        return Validate(await JsonSerializer.DeserializeAsync(stream, SourceGenerationContext.Default.ModuleSearchConfiguration, cancellationToken));
    }


    /// <summary>
    /// Parses a module search config file from a stream.
    /// </summary>
    /// <param name="stream">Stream of UTF-8 encoded JSON.</param>
    /// <returns>Search configuration.</returns>
    public static ModuleSearchConfiguration ParseUtf8Stream(Stream stream)
    {
        return Validate(JsonSerializer.Deserialize(stream, SourceGenerationContext.Default.ModuleSearchConfiguration));
    }

    private static ModuleSearchConfiguration Validate(ModuleSearchConfiguration? moduleSearchConfiguration)
    {
        if (moduleSearchConfiguration == null)
        {
            throw new InvalidDataException("Null JSON is disallowed");
        }
        for (int i = 0; i < moduleSearchConfiguration.Entries.Length; i++)
        {
            ModuleSearchConfigurationEntry entry = moduleSearchConfiguration.Entries[i];
            if (string.IsNullOrEmpty(entry.Path))
            {
                throw new InvalidDataException($"Entry {i} missing path");
            }
            if (string.IsNullOrEmpty(entry.DirectorySuffix))
            {
                throw new InvalidDataException($"Entry {i} missing directory suffix");
            }
            if (string.IsNullOrEmpty(entry.FileNameSuffix))
            {
                throw new InvalidDataException($"Entry {i} missing file name suffix");
            }
        }
        return moduleSearchConfiguration;
    }
}

/// <summary>
/// Represents an entry in a module search configuration.
/// </summary>
/// <param name="Path">Path to the directory containing modules.</param>
/// <param name="Name">Optional name.</param>
/// <param name="DirectorySuffix">Suffix to use for matching directories.</param>
/// <param name="FileNameSuffix">Suffix to use for matching module manifest files.</param>
/// <remarks><paramref name="Path"/> may be relative.</remarks>
public record ModuleSearchConfigurationEntry(
    [property: JsonPropertyName("Path")] string Path,
    [property: JsonPropertyName("Name")] string? Name,
    [property: JsonPropertyName("DirectorySuffix")] string DirectorySuffix,
    [property: JsonPropertyName("FileNameSuffix")] string FileNameSuffix);
