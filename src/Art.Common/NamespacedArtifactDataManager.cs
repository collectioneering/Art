using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Art.Common;

/// <summary>
/// Base type for namespaced artifact data managers.
/// </summary>
public abstract class NamespacedArtifactDataManager : INamespacedArtifactDataManager
{
    /// <inheritdoc />
    public abstract ValueTask<CommittableStream> CreateOutputStreamAsync(string file, string path = "", OutputStreamOptions? options = null, CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public abstract ValueTask<bool> ExistsAsync(string file, string path = "", CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public abstract ValueTask<bool> DeleteAsync(string file, string path = "", CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public abstract ValueTask<Stream> OpenInputStreamAsync(string file, string path = "", CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public abstract ValueTask<string[]> ListFilesAsync(string path, CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public async ValueTask OutputTextAsync(string text, string file, string path = "", OutputStreamOptions? options = null, CancellationToken cancellationToken = default)
    {
        UpdateOptionsTextual(ref options);
        await using CommittableStream stream = await CreateOutputStreamAsync(file, path, options, cancellationToken).ConfigureAwait(false);
        await using var sw = new StreamWriter(stream);
        await sw.WriteAsync(text).ConfigureAwait(false);
        stream.ShouldCommit = true;
    }

    /// <inheritdoc />
    [RequiresUnreferencedCode("JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that takes a JsonTypeInfo or JsonSerializerContext, or make sure all of the required types are preserved.")]
    public async ValueTask OutputJsonAsync<T>(T data, string file, string path = "", OutputStreamOptions? options = null, CancellationToken cancellationToken = default)
    {
        UpdateOptionsTextual(ref options);
        await using CommittableStream stream = await CreateOutputStreamAsync(file, path, options, cancellationToken).ConfigureAwait(false);
        await JsonSerializer.SerializeAsync(stream, data, cancellationToken: cancellationToken).ConfigureAwait(false);
        stream.ShouldCommit = true;
    }

    /// <inheritdoc />
    public async ValueTask OutputJsonAsync<T>(T data, JsonTypeInfo<T> jsonTypeInfo, string file, string path = "", OutputStreamOptions? options = null, CancellationToken cancellationToken = default)
    {
        UpdateOptionsTextual(ref options);
        await using CommittableStream stream = await CreateOutputStreamAsync(file, path, options, cancellationToken).ConfigureAwait(false);
        await JsonSerializer.SerializeAsync(stream, data, jsonTypeInfo, cancellationToken: cancellationToken).ConfigureAwait(false);
        stream.ShouldCommit = true;
    }

    /// <inheritdoc />
    [RequiresUnreferencedCode("JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that takes a JsonTypeInfo or JsonSerializerContext, or make sure all of the required types are preserved.")]
    public async ValueTask OutputJsonAsync<T>(T data, JsonSerializerOptions jsonSerializerOptions, string file, string path = "", OutputStreamOptions? options = null, CancellationToken cancellationToken = default)
    {
        UpdateOptionsTextual(ref options);
        await using CommittableStream stream = await CreateOutputStreamAsync(file, path, options, cancellationToken).ConfigureAwait(false);
        await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions, cancellationToken).ConfigureAwait(false);
        stream.ShouldCommit = true;
    }

    /// <inheritdoc />
    public virtual ValueTask<Checksum> ComputeChecksumAsync(string checksumId, string file, string path = "", CancellationToken cancellationToken = default)
    {
        return ChecksumUtility.ComputeChecksumAsync(this, file, path, checksumId, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async ValueTask<Checksum?> GetChecksumAsync(string file, string path = "", CancellationToken cancellationToken = default)
    {
        if (!await ExistsAsync(file, path, cancellationToken).ConfigureAwait(false)) throw new KeyNotFoundException();
        return null;
    }

    private static void UpdateOptionsTextual(ref OutputStreamOptions? options)
    {
        if (options is { } optionsActual) options = optionsActual with { PreallocationSize = 0 };
    }
}
