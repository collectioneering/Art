namespace Art.Management;

/// <summary>
/// Represents a simple <see cref="ArtifactDataManager"/> with purely file-based tracking.
/// </summary>
public class DiskArtifactDataManager : ArtifactDataManager
{
    /// <summary>
    /// Base directory.
    /// </summary>
    public string BaseDirectory { get; }

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
        try
        {
            return ValueTask.FromResult((Stream)File.OpenRead(Path.Combine(DiskPaths.GetBasePath(BaseDirectory, key.Artifact.Tool, key.Artifact.Group), key.Path, key.File.SafeifyFileName())));
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
        return ValueTask.FromResult(File.Exists(Path.Combine(DiskPaths.GetBasePath(BaseDirectory, key.Artifact.Tool, key.Artifact.Group), key.Path, key.File.SafeifyFileName())));
    }

    /// <inheritdoc/>
    public override ValueTask<bool> DeleteAsync(ArtifactResourceKey key, CancellationToken cancellationToken = default)
    {
        string file = Path.Combine(DiskPaths.GetBasePath(BaseDirectory, key.Artifact.Tool, key.Artifact.Group), key.Path, key.File.SafeifyFileName());
        if (!File.Exists(file)) return ValueTask.FromResult(false);
        File.Delete(file);
        return ValueTask.FromResult(!File.Exists(file));
    }

    /// <inheritdoc/>
    public override ValueTask<CommittableStream> CreateOutputStreamAsync(ArtifactResourceKey key, OutputStreamOptions? options = null, CancellationToken cancellationToken = default)
    {
        string dir = Path.Combine(DiskPaths.GetBasePath(BaseDirectory, key.Artifact.Tool, key.Artifact.Group), key.Path);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        FileStreamOptions fso = new() { Mode = FileMode.Create, Access = FileAccess.ReadWrite };
        if (options is { } optionsActual)
        {
            long preallocationSize = optionsActual.PreallocationSize;
            if (preallocationSize < 0) throw new ArgumentException($"Invalid {nameof(OutputStreamOptions.PreallocationSize)} value", nameof(options));
            if (preallocationSize != 0) fso.PreallocationSize = preallocationSize;
        }
        return new ValueTask<CommittableStream>(new CommittableFileStream(Path.Combine(dir, key.File.SafeifyFileName()), fso));
    }
}
