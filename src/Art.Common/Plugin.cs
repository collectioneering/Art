using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

namespace Art.Common;

/// <summary>
/// Represents a plugin containing <see cref="IArtifactTool"/> implementations.
/// </summary>
[RequiresUnreferencedCode("Loading artifact tools might require types that cannot be statically analyzed.")]
public class Plugin : IArtifactToolSelectableRegistry<string>
{
    /// <summary>
    /// <see cref="AssemblyLoadContext"/>.
    /// </summary>
    public AssemblyLoadContext Context { get; }

    /// <summary>
    /// <see cref="Assembly"/> to draw from.
    /// </summary>
    public Assembly BaseAssembly { get; }

    private List<ArtifactToolSelectableRegistryEntry>? _selectableRegistryEntries;

    /// <summary>
    /// Initializes an instance of <see cref="Plugin"/>.
    /// </summary>
    /// <param name="context"><see cref="AssemblyLoadContext"/>.</param>
    /// <param name="baseAssembly"><see cref="Assembly"/> to draw from.</param>
    public Plugin(AssemblyLoadContext context, Assembly baseAssembly)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }
        if (baseAssembly == null)
        {
            throw new ArgumentNullException(nameof(baseAssembly));
        }
        Context = context;
        BaseAssembly = baseAssembly;
    }

    /// <summary>
    /// Initializes an instance of <see cref="Plugin"/>.
    /// </summary>
    /// <param name="baseAssembly"><see cref="Assembly"/> to draw from.</param>
    public Plugin(Assembly baseAssembly) : this(ResolveAssemblyLoadContext(baseAssembly), baseAssembly)
    {
    }

    private static AssemblyLoadContext ResolveAssemblyLoadContext(Assembly baseAssembly, [CallerArgumentExpression("baseAssembly")] string? argumentName = null)
    {
        return AssemblyLoadContext.GetLoadContext(baseAssembly) ?? throw new ArgumentException("Cannot get load context for an baseAssembly not provided by runtime", argumentName);
    }

    /// <inheritdoc />
    public bool Contains(ArtifactToolID artifactToolId)
    {
        try
        {
            Assembly assembly = Context.LoadFromAssemblyName(new AssemblyName(artifactToolId.Assembly));
            return assembly.GetType(artifactToolId.Type) != null;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public bool TryGetType(ArtifactToolID artifactToolId, [NotNullWhen(true)] out Type? type)
    {
        return ArtifactToolLoader.TryGetType(Context, artifactToolId, out type);
    }

    /// <inheritdoc />
    public bool TryLoad(ArtifactToolID artifactToolId, [NotNullWhen(true)] out IArtifactTool? tool)
    {
        return ArtifactToolLoader.TryLoad(Context, artifactToolId, out tool);
    }

    /// <inheritdoc />
    public IEnumerable<ArtifactToolDescription> GetToolDescriptions()
    {
        return BaseAssembly.GetExportedTypes()
            .Where(t => t.IsAssignableTo(typeof(IArtifactTool)) && !t.IsAbstract && t.GetConstructor(Array.Empty<Type>()) != null)
            .Select(v => new ArtifactToolDescription(v, ArtifactToolIDUtil.CreateToolID(v)));
    }

    /// <inheritdoc />
    public bool TryIdentify(string key, [NotNullWhen(true)] out ArtifactToolID? artifactToolId, [NotNullWhen(true)] out string? artifactId)
    {
        EnsureSelectableRegistryEntriesLoaded();
        foreach (var entry in _selectableRegistryEntries)
        {
            if (entry.TryIdentify(key, out artifactToolId, out artifactId))
            {
                return true;
            }
        }
        artifactToolId = null;
        artifactId = null;
        return false;
    }

    [MemberNotNull(nameof(_selectableRegistryEntries))]
    private void EnsureSelectableRegistryEntriesLoaded()
    {
        if (_selectableRegistryEntries == null)
        {
            _selectableRegistryEntries = new List<ArtifactToolSelectableRegistryEntry>();
            object[] paramArr = new object[1];
            foreach (var x in BaseAssembly.GetExportedTypes()
                         .Where(t => t.IsAssignableTo(typeof(IArtifactTool)) && !t.IsAbstract && t.GetConstructor(Array.Empty<Type>()) != null))
            {
                try
                {
                    var entryType = typeof(ArtifactToolSelectableRegistryEntry<>).MakeGenericType(x);
                    paramArr[0] = ArtifactToolIDUtil.CreateToolID(x);
                    if (Activator.CreateInstance(entryType, paramArr) is ArtifactToolSelectableRegistryEntry entry)
                    {
                        _selectableRegistryEntries.Add(entry);
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }
    }
}
