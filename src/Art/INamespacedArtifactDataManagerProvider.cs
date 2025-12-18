using System.Diagnostics.CodeAnalysis;

namespace Art;

/// <summary>
/// Provides access to <see cref="INamespacedArtifactDataManager"/> by namespace.
/// </summary>
public interface INamespacedArtifactDataManagerProvider<in TNamespace>
{
    /// <summary>
    /// Attempts to get a namespaced data manager for the specified namespace.
    /// </summary>
    /// <param name="targetNamespace">Target namespace.</param>
    /// <param name="manager">Namespaced data manager.</param>
    /// <param name="attemptCreationIfMissing">If true, attempt to create the namespace if it is missing.</param>
    /// <returns>True if successful.</returns>
    bool TryGetNamespacedArtifactDataManager(TNamespace targetNamespace, [NotNullWhen(true)] out INamespacedArtifactDataManager? manager, bool attemptCreationIfMissing);
}
