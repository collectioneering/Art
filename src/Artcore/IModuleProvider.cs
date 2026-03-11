namespace Artcore;

/// <summary>
/// Represents a provider for loading modules.
/// </summary>
public interface IModuleProvider<out TModule>
    : IModuleLookup, IModuleLoader<TModule>;

/// <summary>
/// Represents a provider for loading modules.
/// </summary>
public interface IModuleProvider<TModuleLocation, out TModule>
    : IModuleProvider<TModule>, IModuleLookup<TModuleLocation>
    where TModuleLocation : IModuleLocation;
