namespace Artcore;

/// <summary>
/// Represents a provider for loading modules.
/// </summary>
public interface IModuleProvider<out TModule> : IModuleLoader<TModule>, IModuleLookup;
