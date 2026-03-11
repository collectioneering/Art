namespace Artcore;

/// <summary>
///     Represents a provider for loading modules.
/// </summary>
public interface IModuleLoader<out TModule>
{
    /// <summary>
    /// Checks if the module location can be loaded as a module.
    /// </summary>
    /// <param name="moduleLocation">Module location.</param>
    /// <returns>True the module location appears to be compatible with this instance.</returns>
    bool CanLoadModule(IModuleLocation moduleLocation);

    /// <summary>
    /// Loads a module.
    /// </summary>
    /// <param name="moduleLocation">Module location.</param>
    /// <returns>True if successful.</returns>
    /// <exception cref="ArgumentException">Thrown for invalid type.</exception>
    TModule LoadModule(IModuleLocation moduleLocation);
}
