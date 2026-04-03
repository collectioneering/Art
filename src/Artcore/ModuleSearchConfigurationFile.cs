namespace Artcore;

/// <summary>
/// Represents a module search configuration associated with a specific directory.
/// </summary>
/// <param name="Configuration">Configuration.</param>
/// <param name="Directory">Directory for relative paths in configuration.</param>
public record ModuleSearchConfigurationFile(
    ModuleSearchConfiguration Configuration, string Directory);
