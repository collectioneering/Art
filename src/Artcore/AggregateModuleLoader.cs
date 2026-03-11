namespace Artcore;

/// <summary>
/// Provides prioritized access to multiple <see cref="IModuleLoader{TModule}"/>.
/// </summary>
public class AggregateModuleLoader<TModule> : IModuleLoader<TModule>
{
    private readonly IModuleLoader<TModule>[] _loaders;

    /// <summary>
    /// Initializes an instance of <see cref="AggregateModuleProvider{TModule}"/>.
    /// </summary>
    /// <param name="providers"></param>
    public AggregateModuleLoader(IReadOnlyList<IModuleLoader<TModule>> providers)
    {
        _loaders = providers.ToArray();
    }

    /// <inheritdoc />
    public bool CanLoadModule(IModuleLocation moduleLocation)
    {
        foreach (var loader in _loaders)
        {
            if (loader.CanLoadModule(moduleLocation))
            {
                return true;
            }
        }
        return false;
    }

    /// <inheritdoc />
    public TModule LoadModule(IModuleLocation moduleLocation)
    {
        foreach (var loader in _loaders)
        {
            if (loader.CanLoadModule(moduleLocation))
            {
                return loader.LoadModule(moduleLocation);
            }
        }
        throw new ArgumentException("Cannot load module location, it is of an invalid type.");
    }
}
