namespace Art.Common.Management;

/// <summary>
/// Represents an in-memory artifact registration manager.
/// </summary>
public class InMemoryArtifactRegistrationManager : IArtifactRegistrationManager
{
    private readonly Dictionary<ArtifactKey, ArtifactInfo> _artifacts = new();

    private readonly Dictionary<ArtifactKey, Dictionary<ArtifactResourceKey, ArtifactResourceInfo>> _resources = new();
    private bool _disposed;

    /// <inheritdoc />
    public Task<List<ArtifactInfo>> ListArtifactsAsync(CancellationToken cancellationToken = new())
    {
        EnsureNotDisposed();
        return Task.FromResult(_artifacts.Values.ToList());
    }

    /// <inheritdoc />
    public Task<List<ArtifactInfo>> ListArtifactsAsync(Func<ArtifactInfo, bool> predicate, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        return Task.FromResult(_artifacts.Values.Where(predicate).ToList());
    }

    /// <inheritdoc />
    public Task<List<ArtifactInfo>> ListArtifactsAsync(string tool, CancellationToken cancellationToken = new())
    {
        EnsureNotDisposed();
        return Task.FromResult(_artifacts.Values.Where(v => v.Key.Tool == tool).ToList());
    }

    /// <inheritdoc />
    public Task<List<ArtifactInfo>> ListArtifactsAsync(string tool, string group, CancellationToken cancellationToken = new())
    {
        EnsureNotDisposed();
        return Task.FromResult(_artifacts.Values.Where(v => v.Key.Tool == tool && v.Key.Group == group).ToList());
    }

    /// <inheritdoc />
    public Task<List<ArtifactResourceInfo>> ListResourcesAsync(ArtifactKey key, CancellationToken cancellationToken = new())
    {
        EnsureNotDisposed();
        if (_resources.TryGetValue(key, out var dict))
        {
            return Task.FromResult(dict.Values.ToList());
        }
        return Task.FromResult(new List<ArtifactResourceInfo>());
    }

    /// <inheritdoc />
    public ValueTask AddArtifactAsync(ArtifactInfo artifactInfo, CancellationToken ct = default)
    {
        EnsureNotDisposed();
        _artifacts[artifactInfo.Key] = artifactInfo;
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask<ArtifactInfo?> TryGetArtifactAsync(ArtifactKey key, CancellationToken ct = default)
    {
        EnsureNotDisposed();
        return new ValueTask<ArtifactInfo?>(_artifacts.TryGetValue(key, out ArtifactInfo? value) ? value : null);
    }

    /// <inheritdoc />
    public ValueTask AddResourceAsync(ArtifactResourceInfo artifactResourceInfo, CancellationToken ct = default)
    {
        EnsureNotDisposed();
        if (!_resources.TryGetValue(artifactResourceInfo.Key.Artifact, out var dict))
        {
            _resources.Add(artifactResourceInfo.Key.Artifact, dict = new Dictionary<ArtifactResourceKey, ArtifactResourceInfo>());
        }
        dict.Add(artifactResourceInfo.Key, artifactResourceInfo);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask<ArtifactResourceInfo?> TryGetResourceAsync(ArtifactResourceKey key, CancellationToken ct = default)
    {
        EnsureNotDisposed();
        return new ValueTask<ArtifactResourceInfo?>(_resources.TryGetValue(key.Artifact, out var dict) && dict.TryGetValue(key, out var value) ? value : null);
    }

    /// <inheritdoc />
    public ValueTask RemoveArtifactAsync(ArtifactKey key, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        _artifacts.Remove(key);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask RemoveResourceAsync(ArtifactResourceKey key, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        if (_resources.TryGetValue(key.Artifact, out var dict))
        {
            dict.Remove(key);
        }
        return ValueTask.CompletedTask;
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
}
