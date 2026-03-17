namespace Artcore;

/// <summary>
/// Provides prioritized access to multiple <see cref="IModuleLoader{TModule}"/>.
/// </summary>
/// <remarks>
/// CanLoadModule and LoadModule use FIFO order for loaders.
/// </remarks>
public class AggregateModuleLoader<TModule> : IModuleLoader<TModule>
{
    private readonly IModuleLoader<TModule>[] _loaders;

    /// <summary>
    /// Initializes an instance of <see cref="AggregateModuleProvider{TModule}"/>.
    /// </summary>
    /// <param name="loaders">Individual loaders, with earlier entries having precedence.</param>
    public AggregateModuleLoader(IReadOnlyList<IModuleLoader<TModule>> loaders)
    {
        _loaders = loaders.ToArray();
    }

    /// <inheritdoc />
    /// <remarks>
    /// This method uses FIFO order for the underlying loaders.
    /// </remarks>
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
    /// <remarks>
    /// This method uses FIFO order for the underlying loaders.
    /// </remarks>
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
