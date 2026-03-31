using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Art.Common.Management;

/// <summary>
/// Represents a simple <see cref="IArtifactRegistrationManager"/> with purely file-based tracking.
/// </summary>
public class DiskArtifactRegistrationManager : IArtifactRegistrationManager
{
    private const string ArtifactDir = ".artifacts";
    private const string ArtifactFileName = "{0}.json";
    private const string ArtifactFileNameEnd = ".json";
    internal const string ResourceDir = ".resources";
    private const string ResourceFileName = "{0}.RTFTRSRC.json";
    private const string ResourceFileNameEnd = ".RTFTRSRC.json";

    /// <summary>
    /// Base directory.
    /// </summary>
    public string BaseDirectory { get; }

    private readonly ArtifactRegistrationBaseDirectoryContext _baseDirectoryContext;
    private bool _disposed;

    /// <summary>
    /// Creates a new instance of <see cref="DiskArtifactDataManager"/>.
    /// </summary>
    /// <param name="baseDirectory">Base directory.</param>
    public DiskArtifactRegistrationManager(string baseDirectory)
    {
        BaseDirectory = baseDirectory;
        _baseDirectoryContext = new ArtifactRegistrationBaseDirectoryContext(baseDirectory);
    }

    /// <inheritdoc/>
    public async ValueTask AddArtifactAsync(ArtifactInfo artifactInfo, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        string dir = GetArtifactInfoDir(artifactInfo.Key);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        string path = GetArtifactInfoFilePath(dir, artifactInfo.Key);
        await WriteToFileAsync(artifactInfo, SourceGenerationContext.s_context.ArtifactInfo, path, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<List<ArtifactInfo>> ListArtifactsAsync(CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        string dir = GetArtifactInfoDir();
        List<ArtifactInfo> results = [];
        if (!Directory.Exists(dir)) return results;
        foreach (string toolDir in Directory.EnumerateDirectories(dir))
        foreach (string groupDir in Directory.EnumerateDirectories(toolDir))
        foreach (string file in Directory.EnumerateFiles(groupDir).Where(v => v.EndsWith(ArtifactFileNameEnd)))
            if (await LoadFromFileAsync(file, SourceGenerationContext.s_context.ArtifactInfo, cancellationToken).ConfigureAwait(false) is { } v)
                results.Add(v);
        return results;
    }

    /// <inheritdoc/>
    public async Task<List<ArtifactInfo>> ListArtifactsAsync(Func<ArtifactInfo, bool> predicate, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        string dir = GetArtifactInfoDir();
        List<ArtifactInfo> results = [];
        if (!Directory.Exists(dir)) return results;
        foreach (string toolDir in Directory.EnumerateDirectories(dir))
        foreach (string groupDir in Directory.EnumerateDirectories(toolDir))
        foreach (string file in Directory.EnumerateFiles(groupDir).Where(v => v.EndsWith(ArtifactFileNameEnd)))
            if (await LoadFromFileAsync(file, SourceGenerationContext.s_context.ArtifactInfo, cancellationToken).ConfigureAwait(false) is { } v && predicate(v))
                results.Add(v);
        return results;
    }

    /// <inheritdoc/>
    public async Task<List<ArtifactInfo>> ListArtifactsAsync(string tool, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        string toolDir = GetArtifactInfoDir(tool);
        List<ArtifactInfo> results = [];
        if (!Directory.Exists(toolDir)) return results;
        foreach (string groupDir in Directory.EnumerateDirectories(toolDir))
        foreach (string file in Directory.EnumerateFiles(groupDir).Where(v => v.EndsWith(ArtifactFileNameEnd)))
            if (await LoadFromFileAsync(file, SourceGenerationContext.s_context.ArtifactInfo, cancellationToken).ConfigureAwait(false) is { } v)
                results.Add(v);
        return results;
    }

    /// <inheritdoc/>
    public async Task<List<ArtifactInfo>> ListArtifactsAsync(string tool, string group, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        string groupDir = GetArtifactInfoDir(tool, group);
        List<ArtifactInfo> results = [];
        if (!Directory.Exists(groupDir)) return results;
        foreach (string file in Directory.EnumerateFiles(groupDir).Where(v => v.EndsWith(ArtifactFileNameEnd)))
            if (await LoadFromFileAsync(file, SourceGenerationContext.s_context.ArtifactInfo, cancellationToken).ConfigureAwait(false) is { } v)
                results.Add(v);
        return results;
    }

    /// <inheritdoc/>
    public async ValueTask AddResourceAsync(ArtifactResourceInfo artifactResourceInfo, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        string dir = GetResourceInfoDir(artifactResourceInfo.Key.Artifact, artifactResourceInfo.Key.Path);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        string path = GetResourceInfoFilePath(dir, artifactResourceInfo.Key);
        await WriteToFileAsync(artifactResourceInfo, SourceGenerationContext.s_context.ArtifactResourceInfo, path, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<List<ArtifactResourceInfo>> ListResourcesAsync(ArtifactKey key, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        string dir = GetResourceInfoDir(key);
        List<ArtifactResourceInfo> results = [];
        Queue<string> dQueue = new();
        dQueue.Enqueue(dir);
        while (dQueue.TryDequeue(out string? dd))
        {
            foreach (string f in Directory.EnumerateFiles(dd).Where(v => v.EndsWith(ResourceFileNameEnd)))
                if (await LoadFromFileAsync(f, SourceGenerationContext.s_context.ArtifactResourceInfo, cancellationToken).ConfigureAwait(false) is { } v)
                    results.Add(v);
            foreach (string d in Directory.EnumerateDirectories(dd))
                dQueue.Enqueue(d);
        }
        return results;
    }

    /// <inheritdoc/>
    public async ValueTask<ArtifactInfo?> TryGetArtifactAsync(ArtifactKey key, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        string dir = GetArtifactInfoDir(key);
        string path = GetArtifactInfoFilePath(dir, key);
        return File.Exists(path) ? await LoadFromFileAsync(path, SourceGenerationContext.s_context.ArtifactInfo, cancellationToken).ConfigureAwait(false) : null;
    }

    /// <inheritdoc/>
    public async ValueTask<ArtifactResourceInfo?> TryGetResourceAsync(ArtifactResourceKey key, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        string dir = GetResourceInfoDir(key.Artifact, key.Path);
        string path = GetResourceInfoFilePath(dir, key);
        return File.Exists(path) ? await LoadFromFileAsync(path, SourceGenerationContext.s_context.ArtifactResourceInfo, cancellationToken).ConfigureAwait(false) : null;
    }

    /// <inheritdoc/>
    public ValueTask RemoveArtifactAsync(ArtifactKey key, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        string dir = GetArtifactInfoDir(key);
        string path = GetArtifactInfoFilePath(dir, key);
        if (File.Exists(path))
            File.Delete(path);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc/>
    public ValueTask RemoveResourceAsync(ArtifactResourceKey key, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        string dir = GetResourceInfoDir(key.Artifact, key.Path);
        string path = GetResourceInfoFilePath(dir, key);
        if (File.Exists(path))
            File.Delete(path);
        return ValueTask.CompletedTask;
    }

    private string GetArtifactInfoDir(ArtifactKey key)
    {
        return GetArtifactInfoDir(key.Tool, key.Group);
    }

    private string GetArtifactInfoDir()
    {
        return _baseDirectoryContext.GetSubPath(ArtifactDir);
    }

    private string GetArtifactInfoDir(string tool)
    {
        return _baseDirectoryContext.GetSubPath(ArtifactDir, tool);
    }

    private string GetArtifactInfoDir(string tool, string group)
    {
        return _baseDirectoryContext.GetSubPath(ArtifactDir, tool, group);
    }

    private string GetArtifactInfoFilePath(string dir, ArtifactKey key)
    {
        return _baseDirectoryContext.JoinValidated(dir, string.Format(CultureInfo.InvariantCulture, ArtifactFileName, key.Id));
    }

    private string GetResourceInfoDir(ArtifactKey key)
    {
        return _baseDirectoryContext.GetSubPath(ResourceDir, key.Tool, key.Group, key.Id);
    }

    private string GetResourceInfoDir(ArtifactKey key, string path)
    {
        return _baseDirectoryContext.JoinValidated(_baseDirectoryContext.GetSubPath(ResourceDir, key.Tool, key.Group, key.Id), path);
    }

    private string GetResourceInfoFilePath(string dir, ArtifactResourceKey key)
    {
        return _baseDirectoryContext.JoinValidated(dir, string.Format(CultureInfo.InvariantCulture, ResourceFileName, key.File.SafeifyFileName()));
    }

    /// <summary>
    /// Writes an object to a JSON file.
    /// </summary>
    /// <typeparam name="T">Data type.</typeparam>
    /// <param name="value">Value to write.</param>
    /// <param name="jsonTypeInfo">JSON type info.</param>
    /// <param name="file">File path to write to.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    private static async ValueTask WriteToFileAsync<T>(T value, JsonTypeInfo<T> jsonTypeInfo, string file, CancellationToken cancellationToken = default)
    {
        await using FileStream fs = File.Create(file);
        await JsonSerializer.SerializeAsync(fs, value, jsonTypeInfo, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private void EnsureNotDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }


    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
    }

    /// <summary>
    /// Loads an object from a JSON file.
    /// </summary>
    /// <typeparam name="T">Data type.</typeparam>
    /// <param name="file">File path to load from.</param>
    /// <param name="jsonTypeInfo">JSON type info.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning read data.</returns>
    private static async Task<T?> LoadFromFileAsync<T>(string file, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellationToken = default) => JsonSerializer.Deserialize(await File.ReadAllTextAsync(file, cancellationToken).ConfigureAwait(false), jsonTypeInfo);
}
