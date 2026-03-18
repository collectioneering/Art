using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Loader;

namespace Art.Common;

internal static class PluginMetadataUtility
{
    public static SortedDictionary<string, string> CreatePropertyDictionary()
    {
        return new SortedDictionary<string, string>();
    }

    [RequiresUnreferencedCode("Reflecting on ALC type might require types that cannot be statically analyzed.")]
    public static void AddLocalMetadata(Type type, Assembly baseAssembly, IDictionary<string, string> metadata)
    {
        if (GetBaseAssemblyLocation(baseAssembly) is { } baseAssemblyLocation)
        {
            metadata["AssemblyLocation"] = baseAssemblyLocation;
        }
        if (GetPluginLocation(baseAssembly) is { } pluginLocation)
        {
            metadata["PluginLocation"] = pluginLocation;
        }
    }

    public static string? GetBaseAssemblyLocation(Assembly baseAssembly)
    {
        try
        {
            return baseAssembly.Location;
        }
        catch
        {
            return null;
        }
    }

    [RequiresUnreferencedCode("Reflecting on ALC type might require types that cannot be statically analyzed.")]
    public static string? GetPluginLocation(Assembly baseAssembly)
    {
        var assemblyLoadContext = AssemblyLoadContext.GetLoadContext(baseAssembly);
        if (assemblyLoadContext != null)
        {
            var assemblyLoadContextType = assemblyLoadContext.GetType();
            if (assemblyLoadContextType.FullName == "Artcore.RestrictedPassthroughAssemblyLoadContext")
            {
                var property = assemblyLoadContextType.GetProperty("BasePath", BindingFlags.Public | BindingFlags.Instance);
                if (property != null && property.CanRead && property.PropertyType == typeof(string))
                {
                    return property.GetValue(assemblyLoadContext) as string;
                }
            }
        }
        return null;
    }
}
