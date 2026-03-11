using System.Diagnostics.CodeAnalysis;

namespace Artcore;

/// <summary>
/// Provides prioritized access to multiple <see cref="IModuleLookup"/>.
/// </summary>
public class AggregateModuleLookup : IModuleLookup
{
    private readonly IModuleLookup[] _lookups;

    /// <summary>
    /// Initializes an instance of <see cref="AggregateModuleLookup"/>.
    /// </summary>
    /// <param name="lookups">Individual lookups, with earlier entries having precedence.</param>
    public AggregateModuleLookup(IReadOnlyList<IModuleLookup> lookups)
    {
        _lookups = lookups.ToArray();
    }

    /// <inheritdoc />
    public bool TryLocateModule(string assembly, [NotNullWhen(true)] out IModuleLocation? moduleLocation)
    {
        foreach (var provider in _lookups)
        {
            if (provider.TryLocateModule(assembly, out var innerModuleLocation))
            {
                moduleLocation = innerModuleLocation;
                return true;
            }
        }
        moduleLocation = null;
        return false;
    }

    /// <inheritdoc />
    public void LoadModuleLocations(IDictionary<string, IModuleLocation> dictionary)
    {
        var innerDictionary = new Dictionary<string, IModuleLocation>();
        foreach (var lookup in _lookups)
        {
            lookup.LoadModuleLocations(innerDictionary);
            foreach (var pair in innerDictionary)
            {
                if (!dictionary.ContainsKey(pair.Key))
                {
                    dictionary.Add(pair.Key, pair.Value);
                }
            }
            innerDictionary.Clear();
        }
    }

    /// <inheritdoc />
    public IEnumerable<IModuleLocation> LoadModuleLocations()
    {
        foreach (var lookup in _lookups)
        {
            foreach (var value in lookup.LoadModuleLocations())
            {
                yield return value;
            }
        }
    }
}
