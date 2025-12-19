using System.Diagnostics.CodeAnalysis;
using Art.Common.Resources;

namespace Art.Common.Management;

/// <summary>
/// Represents an in-memory artifact data manager.
/// </summary>
public class InMemoryArtifactDataManager : ArtifactDataManager, INamespacedArtifactDataManagerProvider<ArtifactKey>
{
    /// <summary>
    /// Mapping of artifact keys to resource info.
    /// </summary>
    public IReadOnlyDictionary<ArtifactKey, List<ArtifactResourceInfo>> Artifacts => _artifacts;

    private readonly Dictionary<ArtifactKey, List<ArtifactResourceInfo>> _artifacts = new();

    /// <summary>
    /// Mapping of resource keys to data.
    /// </summary>
    public IReadOnlyDictionary<ArtifactResourceKey, Stream> Entries => _entries;

    private readonly Dictionary<ArtifactKey, InMemoryArtifactDataManagerArtifactKey> _namespacedToolGroup = new();
    private readonly Dictionary<ArtifactResourceKey, Stream> _entries = new();
    private bool _disposed;

    /// <inheritdoc />
    public override ValueTask<CommittableStream> CreateOutputStreamAsync(ArtifactResourceKey key, OutputStreamOptions? options = null, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        // Create a new output stream. If one already exists for mapping, get rid of it.
        // Since everything uses CommittableMemoryStream, underlying memory stream isn't disposed, so
        // previous buffer is still accessible. Doesn't need to be, but that's how it is right now.
        if (_entries.TryGetValue(key, out Stream? s))
        {
            // Invalidate existing.
            s.Dispose();
            _entries.Remove(key);
        }
        CommittableStream stream;
        if (options is { } optionsActual)
        {
            long preallocationSize = optionsActual.PreallocationSize;
            if (preallocationSize < 0 || preallocationSize > Array.MaxLength) throw new ArgumentException($"Invalid {nameof(OutputStreamOptions.PreallocationSize)} value", nameof(options));
            stream = preallocationSize != 0 ? new CommittableMemoryStream((int)optionsActual.PreallocationSize) : new CommittableMemoryStream();
        }
        else
            stream = new CommittableMemoryStream();
        ArtifactKey ak = key.Artifact;
        if (!_artifacts.TryGetValue(ak, out List<ArtifactResourceInfo>? list))
        {
            _artifacts.Add(ak, list = new List<ArtifactResourceInfo>());
        }
        GetOrCreateNamespacedArtifactDataManager(ak);
        list.Add(new ResultStreamArtifactResourceInfo(stream, key, null, null, null, null));
        _entries[key] = stream;
        return new ValueTask<CommittableStream>(stream);
    }

    /// <inheritdoc />
    public override ValueTask<bool> ExistsAsync(ArtifactResourceKey key, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        return ValueTask.FromResult(_entries.ContainsKey(key));
    }

    /// <inheritdoc />
    public override ValueTask<bool> DeleteAsync(ArtifactResourceKey key, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        bool removed = _entries.Remove(key);
        if (_artifacts.TryGetValue(key.Artifact, out var resources))
        {
            resources.RemoveAll(resource => resource.Key.Equals(key));
            if (resources.Count == 0)
            {
                _artifacts.Remove(key.Artifact);
                _namespacedToolGroup.Remove(key.Artifact);
            }
        }
        return ValueTask.FromResult(removed);
    }

    /// <inheritdoc />
    public override ValueTask<Stream> OpenInputStreamAsync(ArtifactResourceKey key, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        // Use a stream wrapping original buffer, but hide away buffer and make stream read-only
        if (!_entries.TryGetValue(key, out Stream? stream)) throw new KeyNotFoundException();
        if (stream is not CommittableMemoryStream cms)
            throw new InvalidOperationException($"Expected {nameof(CommittableMemoryStream)} but got unexpected stream type {stream.GetType()}");
        MemoryStream oms = cms.MemoryStream;
        return ValueTask.FromResult<Stream>(new MemoryStream(oms.GetBuffer(), 0, (int)oms.Length, false, false));
    }

    private record ResultStreamArtifactResourceInfo(Stream Resource, ArtifactResourceKey Key, string? ContentType, DateTimeOffset? Updated, DateTimeOffset? Retrieved, string? Version)
        : StreamArtifactResourceInfo(Resource, Key, ContentType, Updated, Retrieved, Version);

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

    private InMemoryArtifactDataManagerArtifactKey GetOrCreateNamespacedArtifactDataManager(ArtifactKey targetNamespace)
    {
        if (!_namespacedToolGroup.TryGetValue(targetNamespace, out var managerTyped))
        {
            return _namespacedToolGroup[targetNamespace] = new InMemoryArtifactDataManagerArtifactKey(this, targetNamespace);
        }
        return managerTyped;
    }

    /// <inheritdoc />
    public bool TryGetNamespacedArtifactDataManager(ArtifactKey targetNamespace, [NotNullWhen(true)] out INamespacedArtifactDataManager? manager, bool attemptCreationIfMissing)
    {
        if (_namespacedToolGroup.TryGetValue(targetNamespace, out var managerTyped))
        {
            manager = managerTyped;
            return true;
        }
        if (attemptCreationIfMissing)
        {
            manager = _namespacedToolGroup[targetNamespace] = new InMemoryArtifactDataManagerArtifactKey(this, targetNamespace);
            return true;
        }
        manager = null;
        return false;
    }


    private class InMemoryArtifactDataManagerArtifactKey : NamespacedArtifactDataManager
    {
        private readonly InMemoryArtifactDataManager _manager;
        private readonly ArtifactKey _targetNamespace;
        private bool IsDisposed => _disposed || _manager._disposed;
        private bool _disposed;

        public InMemoryArtifactDataManagerArtifactKey(InMemoryArtifactDataManager manager, ArtifactKey targetNamespace)
        {
            _manager = manager;
            _targetNamespace = targetNamespace;
        }

        public override ValueTask<CommittableStream> CreateOutputStreamAsync(string file, string path = "", OutputStreamOptions? options = null, CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();
            return _manager.CreateOutputStreamAsync(new ArtifactResourceKey(_targetNamespace, file, path), options, cancellationToken);
        }

        public override ValueTask<bool> ExistsAsync(string file, string path = "", CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();
            return _manager.ExistsAsync(new ArtifactResourceKey(_targetNamespace, file, path), cancellationToken);
        }

        public override ValueTask<bool> DeleteAsync(string file, string path = "", CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();
            return _manager.DeleteAsync(new ArtifactResourceKey(_targetNamespace, file, path), cancellationToken);
        }

        public override ValueTask<Stream> OpenInputStreamAsync(string file, string path = "", CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();
            return _manager.OpenInputStreamAsync(new ArtifactResourceKey(_targetNamespace, file, path), cancellationToken);
        }

        public override ValueTask<string[]> ListFilesAsync(string path, CancellationToken cancellationToken = default)
        {
            EnsureNotDisposed();
            if (_manager._artifacts.TryGetValue(_targetNamespace, out var resources))
            {
                return ValueTask.FromResult(resources.Where(resource => resource.Key.Path == path).Select(static resource => resource.Key.File).ToArray());
            }
            return ValueTask.FromResult(Array.Empty<string>());
        }

        private void EnsureNotDisposed()
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
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
}
