using System.Diagnostics.CodeAnalysis;

namespace Artcore;

/// <summary>
/// Provides prioritized access to multiple <see cref="IModuleLookup"/>.
/// </summary>
/// <remarks>
/// TryLocateModule and LoadModuleLocations use FIFO order for lookups.
/// </remarks>
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
    /// <remarks>
    /// This method uses FIFO order for the underlying lookups.
    /// </remarks>
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
    /// <remarks>
    /// This method uses FIFO order for the underlying lookups.
    /// </remarks>
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
