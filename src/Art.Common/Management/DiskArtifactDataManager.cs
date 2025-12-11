namespace Art.Common.Management;

/// <summary>
/// Represents a simple <see cref="IArtifactDataManager"/> with purely file-based tracking.
/// </summary>
public class DiskArtifactDataManager : ArtifactDataManager
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
        try
        {
            return ValueTask.FromResult<Stream>(File.OpenRead(Path.Combine(GetBasePathForArtifact(key.Artifact), key.Path, key.File.SafeifyFileName())));
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

    /// <inheritdoc/>
    public override ValueTask<bool> ExistsAsync(ArtifactResourceKey key, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        return ValueTask.FromResult(File.Exists(Path.Combine(GetBasePathForArtifact(key.Artifact), key.Path, key.File.SafeifyFileName())));
    }

    /// <inheritdoc/>
    public override ValueTask<bool> DeleteAsync(ArtifactResourceKey key, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        string file = Path.Combine(GetBasePathForArtifact(key.Artifact), key.Path, key.File.SafeifyFileName());
        if (!File.Exists(file)) return ValueTask.FromResult(false);
        File.Delete(file);
        return ValueTask.FromResult(!File.Exists(file));
    }

    /// <inheritdoc/>
    public override ValueTask<CommittableStream> CreateOutputStreamAsync(ArtifactResourceKey key, OutputStreamOptions? options = null, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        string dir = Path.Combine(GetBasePathForArtifact(key.Artifact), key.Path);
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
        return new ValueTask<CommittableStream>(new CommittableFileStream(Path.Combine(dir, key.File.SafeifyFileName()), fso, preferTemporaryLocation: preferTemporaryLocation));
    }

    private string GetBasePathForArtifact(ArtifactKey key)
    {
        return DiskPaths.GetBasePath(BaseDirectory, key.Tool, key.Group);
    }

    private void EnsureNotDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(DiskArtifactDataManager));
        }
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
}
