using System.Diagnostics.CodeAnalysis;

namespace Art.Common;

/// <summary>
/// Represents an empty <see cref="IExtensionsContext"/>.
/// </summary>
public class NullExtensionsContext : IExtensionsContext
{
    /// <summary>
    /// Represents a singleton for <see cref="NullExtensionsContext"/>.
    /// </summary>
    public static readonly NullExtensionsContext Instance = new();

    /// <inheritdoc />
    public bool TryGetExtension<T>([NotNullWhen(true)] out T? extension) where T : class
    {
        extension = null;
        return false;
    }
}
