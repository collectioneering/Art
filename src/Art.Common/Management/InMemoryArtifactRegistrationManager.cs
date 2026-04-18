using System.Runtime.CompilerServices;

namespace Art.Common.Management;

/// <summary>
/// Represents an in-memory artifact registration manager.
/// </summary>
public class InMemoryArtifactRegistrationManager : IArtifactRegistrationManager
{
    private readonly Dictionary<ArtifactKey, ArtifactInfo> _artifacts = new();

    private readonly Dictionary<ArtifactKey, Dictionary<ArtifactResourceKey, ArtifactResourceInfo>> _resources = new();
    private readonly bool _isReadOnly;
    private bool _disposed;

    /// <summary>
    /// Initializes an instance of <see cref="InMemoryArtifactRegistrationManager"/>.
    /// </summary>
    public InMemoryArtifactRegistrationManager() : this(false)
    {
    }

    /// <summary>
    /// Initializes an instance of <see cref="InMemoryArtifactRegistrationManager"/>.
    /// </summary>
    /// <param name="isReadOnly">If true, writes to the database are disabled.</param>
    public InMemoryArtifactRegistrationManager(bool isReadOnly)
    {
        _isReadOnly = isReadOnly;
    }


    /// <summary>
    /// Initializes an instance of <see cref="InMemoryArtifactRegistrationManager"/>.
    /// </summary>
    /// <param name="inMemoryArtifactRegistrationManager">An existing <see cref="InMemoryArtifactRegistrationManager"/> to copy content from.</param>
    /// <param name="isReadOnly">If true, writes to the database are disabled.</param>
    public InMemoryArtifactRegistrationManager(InMemoryArtifactRegistrationManager inMemoryArtifactRegistrationManager, bool isReadOnly)
    {
        _isReadOnly = isReadOnly;
        foreach (var v in inMemoryArtifactRegistrationManager._artifacts)
        {
            _artifacts[v.Key] = v.Value;
        }
        foreach (var v in inMemoryArtifactRegistrationManager._resources)
        {
            _resources[v.Key] = new Dictionary<ArtifactResourceKey, ArtifactResourceInfo>(v.Value);
        }
    }

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
        ThrowIfReadOnly();
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
        ThrowIfReadOnly();
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
        ThrowIfReadOnly();
        _artifacts.Remove(key);
        _resources.Remove(key);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask RemoveResourceAsync(ArtifactResourceKey key, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        ThrowIfReadOnly();
        if (_resources.TryGetValue(key.Artifact, out var dict))
        {
            dict.Remove(key);
        }
        return ValueTask.CompletedTask;
    }

    private void ThrowIfReadOnly([CallerMemberName] string? callerMemberName = null)
    {
        if (_isReadOnly)
        {
            throw new InvalidOperationException($"Cannot call {callerMemberName ?? "this member"} because this instance is read-only");
        }
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
