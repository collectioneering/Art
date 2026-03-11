using System.Reflection;
using System.Runtime.Loader;

namespace Artcore;

/// <summary>
/// Represents a module consisting of an <see cref="System.Reflection.Assembly"/> and a hosting <see cref="System.Runtime.Loader.AssemblyLoadContext"/>.
/// </summary>
/// <param name="Assembly">Assembly representing primary entry point of the module.</param>
/// <param name="AssemblyLoadContext">The <see cref="System.Runtime.Loader.AssemblyLoadContext"/> that contains the module.</param>
public record ALCModule(Assembly Assembly, AssemblyLoadContext AssemblyLoadContext) : AssemblyModule(Assembly);
