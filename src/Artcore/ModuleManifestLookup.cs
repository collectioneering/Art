using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Artcore;

/// <summary>
/// Provides lookup of modules stored on disk with manifests.
/// </summary>
public class ModuleManifestLookup : IModuleLookup<ModuleManifest>
{
    private readonly List<ModuleManifest> _manifestsUnfiltered = new();
    private readonly Dictionary<string, ModuleManifest> _manifests = new(StringComparer.InvariantCultureIgnoreCase);
    private readonly string _moduleDirectory;
    private readonly string _directorySuffix;
    private readonly string _fileNameSuffix;
    private readonly Dictionary<string, IReadOnlyList<ModuleManifest>> _cached = new();

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
        return TryFind(assembly, _moduleDirectory, out moduleLocation);
    }

    /// <inheritdoc />
    public void LoadModuleLocations(IDictionary<string, IModuleLocation> dictionary)
    {
        if (!Directory.Exists(_moduleDirectory)) return;
        var tmpDict = new Dictionary<string, ModuleManifest>();
        LoadManifests(_moduleDirectory, tmpDict);
        foreach (var pair in tmpDict)
        {
            dictionary[pair.Key] = pair.Value;
        }
    }

    /// <inheritdoc />
    public IEnumerable<IModuleLocation> LoadModuleLocations()
    {
        return LoadTypedModuleLocations();
    }

    /// <inheritdoc />
    public IEnumerable<ModuleManifest> LoadTypedModuleLocations()
    {
        if (!Directory.Exists(_moduleDirectory)) return Array.Empty<ModuleManifest>();
        LoadManifests(_moduleDirectory, null);
        return new List<ModuleManifest>(_manifestsUnfiltered);
    }

    private IEnumerable<string> EnumerateModuleDirectories(string directory)
    {
        return Directory.EnumerateDirectories(directory, $"*{_directorySuffix}", new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive });
    }

    private IEnumerable<string> EnumerateModuleManifests(string directory)
    {
        return Directory.EnumerateFiles(directory, $"*{_fileNameSuffix}", new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive });
    }

    private bool TryFind(string assembly, string directory, [NotNullWhen(true)] out ModuleManifest? manifest)
    {
        if (_manifests.TryGetValue(assembly, out var moduleManifest))
        {
            manifest = moduleManifest;
            return true;
        }
        if (!Directory.Exists(_moduleDirectory))
        {
            manifest = null;
            return false;
        }
        var tmpDict = new Dictionary<string, ModuleManifest>(StringComparer.InvariantCultureIgnoreCase);
        foreach (string subDirectory in EnumerateModuleDirectories(directory))
        {
            LoadManifestsAtTarget(subDirectory, tmpDict);
            if (tmpDict.TryGetValue(assembly, out manifest))
            {
                return true;
            }
            tmpDict.Clear();
        }
        manifest = null;
        return false;
    }

    private void LoadManifests(string directory, IDictionary<string, ModuleManifest>? dictionary)
    {
        foreach (string subDirectory in EnumerateModuleDirectories(directory))
        {
            LoadManifestsAtTarget(subDirectory, dictionary);
        }
    }

    private void LoadManifestsAtTarget(string directory, IDictionary<string, ModuleManifest>? dictionary)
    {
        string fullPath = Path.GetFullPath(directory);
        if (!_cached.TryGetValue(fullPath, out var outputManifests))
        {
            var manifestsToAdd = new List<ModuleManifest>();
            outputManifests = manifestsToAdd;
            foreach (string file in EnumerateModuleManifests(directory))
            {
                if (TryLoad(file, out var content))
                {
                    manifestsToAdd.Add(new ModuleManifest(directory, content));
                }
            }
            foreach (var manifest in manifestsToAdd)
            {
                _manifests.TryAdd(manifest.Content.Assembly, manifest);
            }
            _manifestsUnfiltered.AddRange(manifestsToAdd);
            _cached[fullPath] = manifestsToAdd;
        }
        if (dictionary == null)
        {
            return;
        }
        foreach (var manifest in outputManifests)
        {
            dictionary.TryAdd(manifest.Content.Assembly, manifest);
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
