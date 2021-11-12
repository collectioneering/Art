﻿using System.Text.Json;

namespace Art;

/// <summary>
/// Provides utility functions.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Loads an object from a UTF-8 JSON stream.
    /// </summary>
    /// <typeparam name="T">Data type.</typeparam>
    /// <param name="stream">Stream to load from.</param>
    /// <returns>Read data.</returns>
    public static T LoadFromUtf8Stream<T>(Stream stream) => JsonSerializer.Deserialize<T>(stream, ArtJsonOptions.JsonOptions)!;

    /// <summary>
    /// Loads an object from a JSON file.
    /// </summary>
    /// <typeparam name="T">Data type.</typeparam>
    /// <param name="file">File path to load from.</param>
    /// <returns>Read data.</returns>
    public static T LoadFromFile<T>(string file) => JsonSerializer.Deserialize<T>(File.ReadAllText(file), ArtJsonOptions.JsonOptions)!;

    /// <summary>
    /// Writes an object to a JSON file.
    /// </summary>
    /// <typeparam name="T">Data type.</typeparam>
    /// <param name="value">Value to write.</param>
    /// <param name="file">File path to write to.</param>
    public static void WriteToFile<T>(this T value, string file)
    {
        using var fs = File.Create(file);
        JsonSerializer.Serialize(fs, value);
        return;
    }

    /// <summary>
    /// Downloads a resource from a <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="client">Web client instance.</param>
    /// <param name="url">URL to download from.</param>
    /// <param name="file">File path.</param>
    /// <param name="lengthCheck">Optional length check to skip download.</param>
    /// <returns>Task.</returns>
    public static async Task DownloadResourceToFileAsync(this HttpClient client, string url, string file, long? lengthCheck = null)
    {
        if (lengthCheck != null && File.Exists(file) && new FileInfo(file).Length == lengthCheck) return;
        using var fr = await client.GetAsync(url);
        fr.EnsureSuccessStatusCode();
        using var fs = File.Create(file);
        await fr.Content.CopyToAsync(fs);
    }
}
