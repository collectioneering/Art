using System.Diagnostics.CodeAnalysis;

namespace Art.Common.Management;

/// <summary>
/// Represents a simple <see cref="IArtifactDataManager"/> with purely file-based tracking.
/// </summary>
public class DiskArtifactDataManager : ArtifactDataManager, INamespacedArtifactDataManagerProvider<ToolGroupNamespace>
{
    /// <summary>
    /// Base directory.
    /// </summary>
    public string BaseDirectory { get; }

    private bool _disposed;

    /// <summary>
    /// Creates a new instance of <see cref="DiskArtifactDataManager"/>.
    /// </summary>
    /// <param name="baseDirectory">Base directory.</param>
    public DiskArtifactDataManager(string baseDirectory)
    {
        BaseDirectory = baseDirectory;
    }

    /// <inheritdoc/>
    public override ValueTask<Stream> OpenInputStreamAsync(ArtifactResourceKey key, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        return ValueTask.FromResult<Stream>(OpenInputStream(GetBasePathForArtifact(key.Artifact), key.File, key.Path));
    }

    /// <inheritdoc/>
    public override ValueTask<bool> ExistsAsync(ArtifactResourceKey key, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        return ValueTask.FromResult(Exists(GetBasePathForArtifact(key.Artifact), key.File, key.Path));
    }

    /// <inheritdoc/>
    public override ValueTask<bool> DeleteAsync(ArtifactResourceKey key, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        return ValueTask.FromResult(Delete(GetBasePathForArtifact(key.Artifact), key.File, key.Path));
    }

    /// <inheritdoc/>
    public override ValueTask<CommittableStream> CreateOutputStreamAsync(ArtifactResourceKey key, OutputStreamOptions? options = null, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        return ValueTask.FromResult<CommittableStream>(CreateOutputStream(GetBasePathForArtifact(key.Artifact), key.File, key.Path, options));
    }

    private string GetBasePathForArtifact(ArtifactKey key)
    {
        return DiskPaths.GetBasePath(BaseDirectory, key.Tool, key.Group);
    }

    private string GetBasePathForToolGroupNamespace(ToolGroupNamespace key)
    {
        return DiskPaths.GetBasePath(BaseDirectory, key.Tool, key.Group);
    }

    private void EnsureNotDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (_disposed)
        {
            return;
        }
        _disposed = true;
    }

    private static FileStream OpenInputStream(string basePath, string file, string path)
    {
        try
        {
            return File.OpenRead(Path.Join(basePath, path, file.SafeifyFileName()));
        }
        catch (FileNotFoundException)
        {
            throw new KeyNotFoundException();
        }
        catch (DirectoryNotFoundException)
        {
            throw new KeyNotFoundException();
        }
    }

    private static bool Exists(string basePath, string file, string path)
    {
        return File.Exists(Path.Join(basePath, path, file.SafeifyFileName()));
    }

    private static bool Delete(string basePath, string file, string path)
    {
        string filePath = Path.Join(basePath, path, file.SafeifyFileName());
        if (!File.Exists(filePath)) return false;
        File.Delete(filePath);
        return !File.Exists(filePath);
    }

    private static CommittableFileStream CreateOutputStream(string basePath, string file, string path, OutputStreamOptions? options)
    {
        string dir = Path.Join(basePath, path);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        FileStreamOptions fso = new() { Mode = FileMode.Create, Access = FileAccess.ReadWrite };
        bool preferTemporaryLocation = true;
        if (options != null)
        {
            long preallocationSize = options.PreallocationSize;
            if (preallocationSize < 0) throw new ArgumentException($"Invalid {nameof(OutputStreamOptions.PreallocationSize)} value", nameof(options));
            if (preallocationSize != 0) fso.PreallocationSize = preallocationSize;
            preferTemporaryLocation = options.PreferTemporaryLocation;
        }
        return new CommittableFileStream(Path.Join(dir, file), fso, preferTemporaryLocation: preferTemporaryLocation);
    }

    private static string[] ListFiles(string basePath, string path)
    {
        var di = new DirectoryInfo(Path.Join(basePath, path));
        if (!di.Exists)
        {
            return Array.Empty<string>();
        }
        return di.GetFiles().Select(static file => file.Name).ToArray();
    }

    /// <inheritdoc />
    public bool TryGetNamespacedArtifactDataManager(ToolGroupNamespace targetNamespace, [NotNullWhen(true)] out INamespacedArtifactDataManager? manager, bool attemptCreationIfMissing)
    {
        var di = new DirectoryInfo(GetBasePathForToolGroupNamespace(targetNamespace));
        if (!di.Exists)
        {
            if (attemptCreationIfMissing)
            {
                try
                {
                    di.Create();
                    manager = new DiskArtifactDataManagerToolWithBasePath(di.FullName);
                    return true;
                }
                catch (IOException)
                {
                    // flow to fail
                }
            }
            manager = null;
            return false;
        }
        manager = new DiskArtifactDataManagerToolWithBasePath(di.FullName);
        return true;
    }

    private class DiskArtifactDataManagerToolWithBasePath : NamespacedArtifactDataManager
    {
        private readonly string _basePath;

        public DiskArtifactDataManagerToolWithBasePath(string basePath)
        {
            _basePath = basePath;
        }

        /// <inheritdoc/>
        public override ValueTask<Stream> OpenInputStreamAsync(string file, string path = "", CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult<Stream>(OpenInputStream(_basePath, file, path));
        }

        /// <inheritdoc/>
        public override ValueTask<bool> ExistsAsync(string file, string path = "", CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(Exists(_basePath, file, path));
        }

        /// <inheritdoc/>
        public override ValueTask<bool> DeleteAsync(string file, string path = "", CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(Delete(_basePath, file, path));
        }

        /// <inheritdoc/>
        public override ValueTask<CommittableStream> CreateOutputStreamAsync(string file, string path = "", OutputStreamOptions? options = null, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult<CommittableStream>(CreateOutputStream(_basePath, file, path, options));
        }

        public override ValueTask<string[]> ListFilesAsync(string path, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(ListFiles(_basePath, path));
        }
    }
}
