using System.Diagnostics.CodeAnalysis;

namespace Artcore;

/// <summary>
/// Represents a provider for loading modules.
/// </summary>
public interface IModuleLookup
{
    /// <summary>
    /// Attempts to locate a module.
    /// </summary>
    /// <param name="assembly">Assembly simple name.</param>
    /// <param name="moduleLocation">Module location, if successful.</param>
    /// <returns>True if successful.</returns>
    bool TryLocateModule(string assembly, [NotNullWhen(true)] out IModuleLocation? moduleLocation);

    /// <summary>
    /// Gets all module locations.
    /// </summary>
    /// <returns>Module locations.</returns>
    IEnumerable<IModuleLocation> LoadModuleLocations();
}

/// <summary>
/// Represents a provider for loading modules.
/// </summary>
/// <typeparam name="TModuleLocation">Module location type.</typeparam>
public interface IModuleLookup<TModuleLocation> : IModuleLookup where TModuleLocation : IModuleLocation
{
    bool IModuleLookup.TryLocateModule(string assembly, [NotNullWhen(true)] out IModuleLocation? moduleLocation)
    {
        bool result = TryLocateModule(assembly, out TModuleLocation? moduleLocationAlt);
        moduleLocation = result ? moduleLocationAlt : null;
        return result;
    }

    /// <summary>
    /// Attempts to locate a module.
    /// </summary>
    /// <param name="assembly">Assembly simple name.</param>
    /// <param name="moduleLocation">Module location, if successful.</param>
    /// <returns>True if successful.</returns>
    bool TryLocateModule(string assembly, [NotNullWhen(true)] out TModuleLocation? moduleLocation);

    /// <summary>
    /// Gets all module locations.
    /// </summary>
    /// <returns>Module locations.</returns>
    IEnumerable<TModuleLocation> LoadTypedModuleLocations();
}
