using System.Diagnostics.CodeAnalysis;

namespace Art;

/// <summary>
/// Provides a context for retrieving extensions.
/// </summary>
public interface IExtensionsContext
{
    /// <summary>
    /// Attempts to retrieve an extension of the specified type.
    /// </summary>
    /// <param name="extension">Extension instance, if available.</param>
    /// <typeparam name="T">Extension type.</typeparam>
    /// <returns>True if successful.</returns>
    bool TryGetExtension<T>([NotNullWhen(true)] out T? extension) where T : class;
}
