using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Art.Common;

/// <summary>
/// Utility for creating tool strings.
/// </summary>
public static class ArtifactToolIDUtil
{
    /// <summary>
    /// Creates a tool ID for the specified tool.
    /// </summary>
    /// <param name="type">Tool type.</param>
    /// <returns>Tool ID.</returns>
    public static ArtifactToolID CreateToolID(Type type)
    {
        var (assemblyName, typeName) = GetAssemblyAndTypeNames(type);
        return new ArtifactToolID(assemblyName, typeName);
    }

    /// <summary>
    /// Creates a tool string for the specified tool.
    /// </summary>
    /// <param name="type">Tool type.</param>
    /// <returns>Tool string.</returns>
    public static string CreateToolString(Type type)
    {
        var (assemblyName, typeName) = GetAssemblyAndTypeNames(type);
        return $"{assemblyName}::{typeName}";
    }

    private static (string assemblyName, string typeName) GetAssemblyAndTypeNames(Type type)
    {
        return (
            assemblyName: type.Assembly.GetName().Name ?? throw new InvalidOperationException(),
            typeName: type.FullName ?? throw new InvalidOperationException());
    }

    /// <summary>
    /// Creates a core tool ID for the specified tool.
    /// </summary>
    /// <param name="type">Tool type.</param>
    /// <returns>Tool ID.</returns>
    public static ArtifactToolID CreateCoreToolID(Type type)
    {
        Type? coreType = type;
        while (coreType != null && coreType.GetCustomAttribute<CoreAttribute>() == null) coreType = coreType.BaseType;
        return CreateToolID(coreType ?? type);
    }

    /// <summary>
    /// Creates a core tool string for the specified tool.
    /// </summary>
    /// <param name="type">Tool type.</param>
    /// <returns>Tool string.</returns>
    public static string CreateCoreToolString(Type type)
    {
        Type? coreType = type;
        while (coreType != null && coreType.GetCustomAttribute<CoreAttribute>() == null) coreType = coreType.BaseType;
        return CreateToolString(coreType ?? type);
    }

    /// <summary>
    /// Creates a tool ID for the specified tool.
    /// </summary>
    /// <typeparam name="TTool">Tool type.</typeparam>
    /// <returns>Tool ID.</returns>
    public static ArtifactToolID CreateCoreToolID<TTool>() where TTool : IArtifactTool
    {
        return CreateCoreToolID(typeof(TTool));
    }

    /// <summary>
    /// Creates a tool string for the specified tool.
    /// </summary>
    /// <typeparam name="TTool">Tool type.</typeparam>
    /// <returns>Tool string.</returns>
    public static string CreateCoreToolString<TTool>() where TTool : IArtifactTool
    {
        return CreateCoreToolString(typeof(TTool));
    }

    /// <summary>
    /// Creates a tool ID for the specified tool.
    /// </summary>
    /// <typeparam name="TTool">Tool type.</typeparam>
    /// <returns>Tool ID.</returns>
    public static ArtifactToolID CreateToolID<TTool>() where TTool : IArtifactTool
    {
        return CreateToolID(typeof(TTool));
    }

    /// <summary>
    /// Creates a tool string for the specified tool.
    /// </summary>
    /// <typeparam name="TTool">Tool type.</typeparam>
    /// <returns>Tool string.</returns>
    public static string CreateToolString<TTool>() where TTool : IArtifactTool
    {
        return CreateToolString(typeof(TTool));
    }

    private static readonly Regex s_toolRegex = new(@"^([\S\s]+)::([\S\s]+)$");

    /// <summary>
    /// Separates assembly and type name from <see cref="ArtifactToolProfile.Tool"/>.
    /// </summary>
    /// <param name="artifactToolProfile">Artifact tool profile.</param>
    /// <returns>Separated assembly and type name.</returns>
    /// <exception cref="ArgumentException">Thrown if this instance has an invalid <see cref="ArtifactToolProfile.Tool"/> value.</exception>
    public static ArtifactToolID GetID(this ArtifactToolProfile artifactToolProfile) => ParseID(artifactToolProfile.Tool);

    /// <summary>
    /// Separates assembly and type name from <see cref="ArtifactToolProfile.Tool"/>.
    /// </summary>
    /// <param name="tool">Artifact tool target string(assembly::toolType)</param>
    /// <param name="artifactToolId">Separated assembly and type name, if successful.</param>
    /// <returns>True if successful.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tool"/> is null.</exception>
    public static bool TryParseID(string tool, [NotNullWhen(true)] out ArtifactToolID? artifactToolId)
    {
        ArgumentNullException.ThrowIfNull(tool);
        if (s_toolRegex.Match(tool) is not { Success: true } match)
        {
            artifactToolId = null;
            return false;
        }

        artifactToolId = new ArtifactToolID(match.Groups[1].Value, match.Groups[2].Value);
        return true;
    }
    /// <summary>
    /// Separates assembly and type name from <see cref="ArtifactToolProfile.Tool"/>.
    /// </summary>
    /// <param name="tool">Artifact tool target string(assembly::toolType)</param>
    /// <returns>Separated assembly and type name.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tool"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown for an invalid <paramref name="tool"/> value.</exception>
    public static ArtifactToolID ParseID(string tool)
    {
        ArgumentNullException.ThrowIfNull(tool);
        if (s_toolRegex.Match(tool) is not { Success: true } match)
        {
            throw new ArgumentException("Tool string is in invalid format, must be \"<assembly>::<toolType>\"", nameof(tool));
        }
        return new ArtifactToolID(match.Groups[1].Value, match.Groups[2].Value);
    }
}
