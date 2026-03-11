using System.Reflection;

namespace Artcore;

/// <summary>
/// Represents a module consisting of an <see cref="System.Reflection.Assembly"/>.
/// </summary>
/// <param name="Assembly">Assembly representing primary entry point of the module.</param>
public record AssemblyModule(Assembly Assembly);
