using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Artcore;

/// <summary>
/// Provides lookup of modules stored on disk with manifests.
/// </summary>
public class ModuleManifestLookup : IModuleLookup<ModuleManifest>
{
    private readonly Dictionary<string, ModuleManifest> _manifests = new(StringComparer.InvariantCultureIgnoreCase);
    private readonly string _moduleDirectory;
    private readonly string _directorySuffix;
    private readonly string _fileNameSuffix;
    private readonly HashSet<string> _searched = new();

    /// <summary>
    /// Creates an instance of <see cref="ModuleManifestLookup"/>.
    /// </summary>
    /// <param name="moduleDirectory">Module directory.</param>
    /// <param name="directorySuffix">Suffix on module directories.</param>
    /// <param name="fileNameSuffix">Suffix on module manifests.</param>
    /// <returns>Instance.</returns>
    public static ModuleManifestLookup Create(
        string moduleDirectory,
        string directorySuffix,
        string fileNameSuffix)
    {
        return new ModuleManifestLookup(moduleDirectory, directorySuffix, fileNameSuffix);
    }

    private ModuleManifestLookup(
        string moduleDirectory,
        string directorySuffix,
        string fileNameSuffix)
    {
        _moduleDirectory = moduleDirectory;
        _directorySuffix = directorySuffix;
        _fileNameSuffix = fileNameSuffix;
    }

    /// <inheritdoc />
    public bool TryLocateModule(string assembly, [NotNullWhen(true)] out ModuleManifest? moduleLocation)
    {
        if (_manifests.TryGetValue(assembly, out var moduleManifest))
        {
            moduleLocation = moduleManifest;
            return true;
        }
        if (!Directory.Exists(_moduleDirectory))
        {
            moduleLocation = null;
            return false;
        }
        if (TryFind(assembly, _moduleDirectory, out moduleManifest, _manifests, _searched))
        {
            moduleLocation = moduleManifest;
            return true;
        }
        moduleLocation = null;
        return false;
    }

    /// <inheritdoc />
    public void LoadModuleLocations(IDictionary<string, IModuleLocation> dictionary)
    {
        if (!Directory.Exists(_moduleDirectory)) return;
        LoadManifests(dictionary, _moduleDirectory, _manifests, _searched);
    }

    private bool TryFind(string assembly, string dir, [NotNullWhen(true)] out ModuleManifest? manifest, IDictionary<string, ModuleManifest>? toAugment = null, ISet<string>? searched = null)
    {
        foreach (string directory in Directory.EnumerateDirectories(dir, $"*{_directorySuffix}", new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive }))
        {
            if (searched != null && !searched.Add(Path.GetFullPath(directory)))
            {
                continue;
            }
            if (TryFindAtTarget(assembly, directory, out manifest, toAugment))
            {
                return true;
            }
        }
        manifest = null;
        return false;
    }

    private bool TryFindAtTarget(string assembly, string directory, [NotNullWhen(true)] out ModuleManifest? manifest, IDictionary<string, ModuleManifest>? toAugment = null)
    {
        foreach (string file in Directory.EnumerateFiles(directory, $"*{_fileNameSuffix}", new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive }))
        {
            if (TryLoad(file, out var content))
            {
                manifest = null;
                if (toAugment != null && !toAugment.ContainsKey(content.Assembly))
                {
                    manifest = new ModuleManifest(directory, content);
                    toAugment.Add(content.Assembly, manifest);
                }
                if (content.Assembly.Equals(assembly, StringComparison.InvariantCultureIgnoreCase))
                {
                    manifest ??= new ModuleManifest(directory, content);
                    return true;
                }
            }
        }
        manifest = null;
        return false;
    }

    private void LoadManifests(IDictionary<string, IModuleLocation> dictionary, string dir, IDictionary<string, ModuleManifest>? toAugment = null, ISet<string>? searched = null)
    {
        if (toAugment != null)
        {
            foreach (var pair in toAugment)
            {
                dictionary[pair.Key] = pair.Value;
            }
        }
        foreach (string directory in Directory.EnumerateDirectories(dir, $"*{_directorySuffix}", new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive }))
        {
            if (searched != null && !searched.Add(Path.GetFullPath(directory)))
            {
                continue;
            }
            LoadManifestsAtTarget(dictionary, directory, toAugment);
        }
    }

    private void LoadManifestsAtTarget(IDictionary<string, IModuleLocation> dictionary, string directory, IDictionary<string, ModuleManifest>? toAugment = null)
    {
        foreach (string file in Directory.EnumerateFiles(directory, $"*{_fileNameSuffix}", new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive }))
        {
            if (TryLoad(file, out var content))
            {
                var manifest = new ModuleManifest(directory, content);
                if (!dictionary.ContainsKey(content.Assembly))
                {
                    dictionary.Add(content.Assembly, manifest);
                }
                if (toAugment != null && !toAugment.ContainsKey(content.Assembly))
                {
                    toAugment.Add(content.Assembly, manifest);
                }
            }
        }
    }

    private static bool TryLoad(string file, [NotNullWhen(true)] out ModuleManifestContent? content)
    {
        try
        {
            content = JsonSerializer.Deserialize(File.ReadAllText(file), SourceGenerationContext.s_context.ModuleManifestContent) ?? throw new IOException($"Failed to deserialize manifest file {file}");
            return true;
        }
        catch
        {
            content = null;
            return false;
        }
    }
}
