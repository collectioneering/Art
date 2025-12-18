using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Art;

/// <summary>
/// Provides namespaced management of resources.
/// </summary>
public interface INamespacedArtifactDataManager
{
    /// <summary>
    /// Creates an output stream for the specified resource.
    /// </summary>
    /// <param name="file">File name.</param>
    /// <param name="path">File path.</param>
    /// <param name="options">Creation options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning a writeable stream to write output to.</returns>
    ValueTask<CommittableStream> CreateOutputStreamAsync(string file, string path = "", OutputStreamOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if data for the specified resource exists.
    /// </summary>
    /// <param name="file">File name.</param>
    /// <param name="path">File path.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning true if resource exists.</returns>
    ValueTask<bool> ExistsAsync(string file, string path = "", CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes data for the specified resource.
    /// </summary>
    /// <param name="file">File name.</param>
    /// <param name="path">File path.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning true if the resource was deleted.</returns>
    ValueTask<bool> DeleteAsync(string file, string path = "", CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a read-only stream for the specified resource.
    /// </summary>
    /// <param name="file">File name.</param>
    /// <param name="path">File path.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning a read-only stream.</returns>
    /// <exception cref="KeyNotFoundException">Thrown for missing resource.</exception>
    ValueTask<Stream> OpenInputStreamAsync(string file, string path = "", CancellationToken cancellationToken = default);

    /// <summary>
    /// Outputs a memory buffer for the specified artifact.
    /// </summary>
    /// <param name="buffer">Buffer to output.</param>
    /// <param name="file">File name.</param>
    /// <param name="path">File path.</param>
    /// <param name="options">Output options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    ValueTask OutputMemoryAsync(ReadOnlyMemory<byte> buffer, string file, string path = "", OutputStreamOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Outputs a text file for the specified artifact.
    /// </summary>
    /// <param name="text">Text to output.</param>
    /// <param name="file">File name.</param>
    /// <param name="path">File path.</param>
    /// <param name="options">Output options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    ValueTask OutputTextAsync(string text, string file, string path = "", OutputStreamOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Outputs a JSON-serialized file for the specified artifact.
    /// </summary>
    /// <param name="data">Data to output.</param>
    /// <param name="file">File name.</param>
    /// <param name="path">File path.</param>
    /// <param name="options">Output options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    [RequiresUnreferencedCode("JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that takes a JsonTypeInfo or JsonSerializerContext, or make sure all of the required types are preserved.")]
    ValueTask OutputJsonAsync<T>(T data, string file, string path = "", OutputStreamOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Outputs a JSON-serialized file for the specified artifact.
    /// </summary>
    /// <param name="data">Data to output.</param>
    /// <param name="jsonTypeInfo">JSON type info.</param>
    /// <param name="file">File name.</param>
    /// <param name="path">File path.</param>
    /// <param name="options">Output options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    ValueTask OutputJsonAsync<T>(T data, JsonTypeInfo<T> jsonTypeInfo, string file, string path = "", OutputStreamOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Outputs a JSON-serialized file for the specified artifact.
    /// </summary>
    /// <param name="data">Data to output.</param>
    /// <param name="jsonSerializerOptions">Serialization options.</param>
    /// <param name="file">File name.</param>
    /// <param name="path">File path.</param>
    /// <param name="options">Output options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    [RequiresUnreferencedCode("JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that takes a JsonTypeInfo or JsonSerializerContext, or make sure all of the required types are preserved.")]
    ValueTask OutputJsonAsync<T>(T data, JsonSerializerOptions jsonSerializerOptions, string file, string path = "", OutputStreamOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Computes checksum of a resource.
    /// </summary>
    /// <param name="checksumId">Checksum algorithm ID.</param>
    /// <param name="file">File name.</param>
    /// <param name="path">File path.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Checksum for resource.</returns>
    /// <exception cref="KeyNotFoundException">Thrown for missing resource.</exception>
    /// <exception cref="ArgumentException">Thrown for a bad <paramref name="checksumId"/> value.</exception>
    ValueTask<Checksum> ComputeChecksumAsync(string checksumId, string file, string path = "", CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets existing checksum associated with a resource if it exists.
    /// </summary>
    /// <param name="file">File name.</param>
    /// <param name="path">File path.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A checksum for the resource, if one exists.</returns>
    /// <exception cref="KeyNotFoundException">Thrown for missing resource.</exception>
    /// <remarks>
    /// This method should return any "primary" checksum readily available from this manager.
    /// </remarks>
    ValueTask<Checksum?> GetChecksumAsync(string file, string path = "", CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists file names in the specified path.
    /// </summary>
    /// <param name="path">File path.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning array of available files.</returns>
    /// <remarks>
    /// Returned array contains file names of recognized content within the specified path, non-recursive.
    /// </remarks>
    ValueTask<string[]> ListFilesAsync(string path, CancellationToken cancellationToken = default);

}
