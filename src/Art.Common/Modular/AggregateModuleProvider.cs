using System.Diagnostics.CodeAnalysis;

namespace Art.Common.Modular;

/// <summary>
/// Provides prioritized access to multiple <see cref="IModuleProvider"/>.
/// </summary>
public class AggregateModuleProvider : IModuleProvider
{
    private readonly IModuleProvider[] _providers;

    /// <summary>
    /// Initializes an instance of <see cref="AggregateModuleProvider"/>.
    /// </summary>
    /// <param name="providers"></param>
    public AggregateModuleProvider(IModuleProvider[] providers)
    {
        _providers = providers;
    }

    /// <inheritdoc />
    public bool TryLocateModule(string assembly, [NotNullWhen(true)] out IModuleLocation? moduleLocation)
    {
        foreach (var provider in _providers)
        {
            if (provider.TryLocateModule(assembly, out var innerModuleLocation))
            {
                moduleLocation = new AggregateModuleLocation(provider, innerModuleLocation);
                return true;
            }
        }
        moduleLocation = null;
        return false;
    }

    private record AggregateModuleLocation(IModuleProvider ModuleProvider, IModuleLocation InnerModuleLocation) : IModuleLocation;

    /// <inheritdoc />
    public IArtifactToolRegistry LoadModule(IModuleLocation moduleLocation)
    {
        if (moduleLocation is not AggregateModuleLocation aggregateModuleLocation)
        {
            throw new ArgumentException("Cannot load module location, it is of an invalid type.");
        }
        return aggregateModuleLocation.ModuleProvider.LoadModule(aggregateModuleLocation.InnerModuleLocation);
    }

    /// <inheritdoc />
    public void LoadModuleLocations(IDictionary<string, IModuleLocation> dictionary)
    {
        var innerDictionary = new Dictionary<string, IModuleLocation>();
        foreach (var provider in _providers)
        {
            provider.LoadModuleLocations(innerDictionary);
            foreach (var pair in innerDictionary)
            {
                if (!dictionary.ContainsKey(pair.Key))
                {
                    dictionary.Add(pair.Key, new AggregateModuleLocation(provider, pair.Value));
                }
            }
            innerDictionary.Clear();
        }
    }
}
