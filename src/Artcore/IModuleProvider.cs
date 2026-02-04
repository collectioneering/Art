using System.Diagnostics.CodeAnalysis;

namespace Artcore;

/// <summary>
/// Represents a provider for loading modules.
/// </summary>
public interface IModuleProvider<out TModule>
{
    /// <summary>
    /// Attempts to locate a module.
    /// </summary>
    /// <param name="assembly">Assembly simple name.</param>
    /// <param name="moduleLocation">Module location, if successful.</param>
    /// <returns>True if successful.</returns>
    bool TryLocateModule(string assembly, [NotNullWhen(true)] out IModuleLocation? moduleLocation);

    /// <summary>
    /// Loads a module.
    /// </summary>
    /// <param name="moduleLocation">Module location.</param>
    /// <returns>True if successful.</returns>
    /// <exception cref="ArgumentException">Thrown for invalid type.</exception>
    TModule LoadModule(IModuleLocation moduleLocation);

    /// <summary>
    /// Gets all module locations.
    /// </summary>
    /// <param name="dictionary">Dictionary to populate.</param>
    void LoadModuleLocations(IDictionary<string, IModuleLocation> dictionary);
}
