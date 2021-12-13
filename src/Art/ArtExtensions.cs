﻿using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Art;

/// <summary>
/// Provides utility functions.
/// </summary>
public static class ArtExtensions
{
    /// <summary>
    /// Loads an object from a UTF-8 JSON stream.
    /// </summary>
    /// <typeparam name="T">Data type.</typeparam>
    /// <param name="stream">Stream to load from.</param>
    /// <returns>Read data.</returns>
    public static T? LoadFromUtf8Stream<T>(Stream stream) => JsonSerializer.Deserialize<T>(stream, ArtJsonOptions.s_jsonOptions);

    /// <summary>
    /// Loads an object from a UTF-8 JSON stream.
    /// </summary>
    /// <typeparam name="T">Data type.</typeparam>
    /// <param name="stream">Stream to load from.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning read data.</returns>
    public static async Task<T?> LoadFromUtf8StreamAsync<T>(Stream stream, CancellationToken cancellationToken = default) => (await JsonSerializer.DeserializeAsync<T>(stream, ArtJsonOptions.s_jsonOptions, cancellationToken).ConfigureAwait(false));

    /// <summary>
    /// Loads an object from a JSON file.
    /// </summary>
    /// <typeparam name="T">Data type.</typeparam>
    /// <param name="file">File path to load from.</param>
    /// <returns>Read data.</returns>
    public static T? LoadFromFile<T>(string file) => JsonSerializer.Deserialize<T>(File.ReadAllText(file), ArtJsonOptions.s_jsonOptions);

    /// <summary>
    /// Loads an object from a JSON file.
    /// </summary>
    /// <typeparam name="T">Data type.</typeparam>
    /// <param name="file">File path to load from.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning ead data.</returns>
    public static async Task<T?> LoadFromFileAsync<T>(string file, CancellationToken cancellationToken = default) => JsonSerializer.Deserialize<T>(await File.ReadAllTextAsync(file, cancellationToken).ConfigureAwait(false), ArtJsonOptions.s_jsonOptions);

    /// <summary>
    /// Writes an object to a JSON file.
    /// </summary>
    /// <typeparam name="T">Data type.</typeparam>
    /// <param name="value">Value to write.</param>
    /// <param name="file">File path to write to.</param>
    public static void WriteToFile<T>(this T value, string file)
    {
        using FileStream fs = File.Create(file);
        JsonSerializer.Serialize(fs, value);
        return;
    }

    /// <summary>
    /// Writes an object to a JSON file.
    /// </summary>
    /// <typeparam name="T">Data type.</typeparam>
    /// <param name="value">Value to write.</param>
    /// <param name="file">File path to write to.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    public static async ValueTask WriteToFileAsync<T>(this T value, string file, CancellationToken cancellationToken = default)
    {
        using FileStream fs = File.Create(file);
        await JsonSerializer.SerializeAsync(fs, value, cancellationToken: cancellationToken).ConfigureAwait(false);
        return;
    }

    /// <summary>
    /// Downloads a resource from a <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="client">Web client instance.</param>
    /// <param name="url">URL to download from.</param>
    /// <param name="file">File path.</param>
    /// <param name="lengthCheck">Optional length check to skip download.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    public static async ValueTask DownloadResourceToFileAsync(this HttpClient client, string url, string file, long? lengthCheck = null, CancellationToken cancellationToken = default)
    {
        if (lengthCheck != null && File.Exists(file) && new FileInfo(file).Length == lengthCheck) return;
        using HttpResponseMessage fr = await client.GetAsync(url, cancellationToken);
        fr.EnsureSuccessStatusCode();
        using FileStream fs = File.Create(file);
        await fr.Content.CopyToAsync(fs, cancellationToken);
    }

    private static readonly char[] s_invalid = Path.GetInvalidFileNameChars();

    /// <summary>
    /// Remove invalid filename characters, based on <see cref="Path.GetInvalidFileNameChars()"/>.
    /// </summary>
    /// <param name="name">String.</param>
    /// <returns>Better filename.</returns>
    public static string SafeifyFileName(this string name) => s_invalid.Aggregate(Path.GetFileName(name), (f, v) => f.Contains(v) ? f.Replace(v, '-') : f);

    /// <summary>
    /// Copies all elements to a list asynchronously.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    /// <param name="enumerable">Enumerable to convert to list.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning list.</returns>
    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> enumerable, CancellationToken cancellationToken = default)
    {
        List<T> list = new();
        await foreach (T value in enumerable.WithCancellation(cancellationToken).ConfigureAwait(false))
            list.Add(value);
        return list;
    }

    /// <summary>
    /// Simplifies a numeric key string.
    /// </summary>
    /// <param name="key">Number key.</param>
    /// <returns>Normalized number key (e.g. 1, 616, 333018).</returns>
    public static string SimplifyNumericKey(this string key)
        => long.TryParse(key, NumberStyles.Integer, CultureInfo.InvariantCulture, out long result) ? result.ToString(CultureInfo.InvariantCulture) : key;

    /// <summary>
    /// Lists artifacts as key-value pairs of ID to artifact data.
    /// </summary>
    /// <param name="artifactTool">Artifact tool.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning created dictionary.</returns>
    public static async ValueTask<Dictionary<string, ArtifactData>> ListDictionaryAsync(this IArtifactToolList artifactTool, CancellationToken cancellationToken = default)
    {
        Dictionary<string, ArtifactData> res = new();
        await foreach (ArtifactData artifactData in artifactTool.ListAsync(cancellationToken).ConfigureAwait(false))
            res[artifactData.Info.Key.Id] = artifactData;
        return res;
    }

    /// <summary>
    /// Takes up to a specified number of elements from an enumerator.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    /// <param name="enumerator">Enumerator.</param>
    /// <param name="collection">Collection to populate.</param>
    /// <param name="max">Maximum number of elements to take.</param>
    /// <returns>True if any elements were taken (false if no more enumerator elements remained).</returns>
    public static bool DoTake<T>(this IEnumerator<T> enumerator, ICollection<T> collection, int max)
    {
        bool any = false;
        while (max-- > 0 && enumerator.MoveNext())
        {
            collection.Add(enumerator.Current);
            any = true;
        }
        return any;
    }

    /// <summary>
    /// Fallback empty enumerable.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    /// <param name="enumerable">Enumerable.</param>
    /// <returns>Nonnull enumerable.</returns>
    public static IEnumerable<T> FallbackEmpty<T>(this IEnumerable<T>? enumerable) => enumerable ?? Array.Empty<T>();

    /// <summary>
    /// Shorthand for ConfigureAwait(false).
    /// </summary>
    /// <param name="task">Task to wrap.</param>
    /// <returns>Wrapped task.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ConfiguredTaskAwaitable Caf(this Task task) => task.ConfigureAwait(false);

    /// <summary>
    /// Shorthand for ConfigureAwait(false).
    /// </summary>
    /// <param name="task">Task to wrap.</param>
    /// <returns>Wrapped task.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ConfiguredTaskAwaitable<T> Caf<T>(this Task<T> task) => task.ConfigureAwait(false);

    /// <summary>
    /// Shorthand for ConfigureAwait(false).
    /// </summary>
    /// <param name="task">Task to wrap.</param>
    /// <returns>Wrapped task.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ConfiguredValueTaskAwaitable Caf(this ValueTask task) => task.ConfigureAwait(false);

    /// <summary>
    /// Shorthand for ConfigureAwait(false).
    /// </summary>
    /// <param name="task">Task to wrap.</param>
    /// <returns>Wrapped task.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ConfiguredValueTaskAwaitable<T> Caf<T>(this ValueTask<T> task) => task.ConfigureAwait(false);

    /// <summary>
    /// Creates a <see cref="JsonElement"/> from this value.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="value">Value.</param>
    /// <returns>JSON element.</returns>
    public static JsonElement J<T>(this T value) => JsonSerializer.SerializeToElement(value);
}
