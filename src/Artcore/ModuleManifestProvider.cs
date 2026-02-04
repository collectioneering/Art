using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;

namespace Artcore;

/// <summary>
/// Provides disk and manifest backed module provider.
/// </summary>
[RequiresUnreferencedCode("Loading modules might require types that cannot be statically analyzed.")]
public class ModuleManifestProvider<TModule> : IModuleProvider<TModule>
{
    private readonly ModuleLoadConfiguration _moduleLoadConfiguration;
    private readonly Dictionary<string, ModuleManifest> _manifests = new(StringComparer.InvariantCultureIgnoreCase);
    private readonly HashSet<string> _searched = new();
    private readonly string _moduleDirectory;
    private readonly string _directorySuffix;
    private readonly string _fileNameSuffix;
    private readonly Func<AssemblyLoadContext, Assembly, TModule> _creationFunction;

    /// <summary>
    /// Creates a provider instance.
    /// </summary>
    /// <param name="moduleLoadConfiguration">Load configuration.</param>
    /// <param name="moduleDirectory">Module directory.</param>
    /// <param name="directorySuffix">Suffix on module directories.</param>
    /// <param name="fileNameSuffix">Suffix on module manifests.</param>
    /// <param name="creationFunction">Creation function.</param>
    /// <returns>Instance.</returns>
    public static ModuleManifestProvider<TModule> Create(
        ModuleLoadConfiguration moduleLoadConfiguration,
        string moduleDirectory,
        string directorySuffix,
        string fileNameSuffix,
        Func<AssemblyLoadContext, Assembly, TModule> creationFunction)
    {
        return new ModuleManifestProvider<TModule>(moduleLoadConfiguration, moduleDirectory, directorySuffix, fileNameSuffix, creationFunction);
    }

    private ModuleManifestProvider(
        ModuleLoadConfiguration moduleLoadConfiguration,
        string moduleDirectory,
        string directorySuffix,
        string fileNameSuffix,
        Func<AssemblyLoadContext, Assembly, TModule> creationFunction)
    {
        _moduleLoadConfiguration = moduleLoadConfiguration;
        _moduleDirectory = moduleDirectory;
        _directorySuffix = directorySuffix;
        _fileNameSuffix = fileNameSuffix;
        _creationFunction = creationFunction;
    }

    /// <inheritdoc />
    public bool TryLocateModule(string assembly, [NotNullWhen(true)] out IModuleLocation? moduleLocation)
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
