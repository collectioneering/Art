namespace Artcore;

/// <summary>
/// Provides a file location (<see cref="File"/>) from which to load a <see cref="ModuleSearchConfiguration"/>
/// and a base directory (<see cref="BaseDirectory"/>) to resolve relative paths against.
/// </summary>
/// <param name="File">Path to a <see cref="ModuleSearchConfiguration"/> file.</param>
/// <param name="BaseDirectory">Base directory to resolve relative paths against.</param>
public record ModuleSearchConfigurationSource(string File, string BaseDirectory);
