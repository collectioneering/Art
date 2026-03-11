namespace Artcore;

/// <summary>
///     Represents a provider for loading modules.
/// </summary>
public interface IModuleLoader<out TModule>
{
    /// <summary>
    ///     Loads a module.
    /// </summary>
    /// <param name="moduleLocation">Module location.</param>
    /// <returns>True if successful.</returns>
    /// <exception cref="ArgumentException">Thrown for invalid type.</exception>
    TModule LoadModule(IModuleLocation moduleLocation);
}
