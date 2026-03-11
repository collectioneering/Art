namespace Artcore;

/// <summary>
/// Represents a manifest for a <see cref="DiskManifestModuleProvider"/> module.
/// </summary>
/// <param name="BasePath">Base path.</param>
/// <param name="Content">Module content.</param>
public record ModuleManifest(string BasePath, ModuleManifestContent Content) : IModuleLocation;
