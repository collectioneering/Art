using System.Linq.Expressions;

namespace Art.EF;

/// <summary>
/// Represents an EF artifact registration manager.
/// </summary>
public class EFArtifactRegistrationManager<TContext> : IArtifactRegistrationManager, IDisposable, IAsyncDisposable where TContext : ArtifactContext
{
    private bool _disposed;

    /// <summary>
    /// Database context.
    /// </summary>
    public TContext Context { get; private set; }

    /// <summary>
    /// Creates a new instance of <see cref="EFArtifactRegistrationManager{T}"/> using the specified factory.
    /// </summary>
    /// <param name="factory">Context factory.</param>
    public EFArtifactRegistrationManager(ArtifactContextFactoryBase<TContext> factory)
    {
        Context = factory.CreateDbContext(Array.Empty<string>());
    }

    /// <inheritdoc />
    public Task<List<ArtifactInfo>> ListArtifactsAsync(CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        return Context.ListArtifactsAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<List<ArtifactInfo>> ListArtifactsAsync(Func<ArtifactInfo, bool> predicate, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        return Context.ListArtifactsAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Lists all artifacts using the specified predicate.
    /// </summary>
    /// <param name="predicate">Predicate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task returning artifacts.</returns>
    public Task<List<ArtifactInfo>> ListArtifactsAsync(Expression<Func<ArtifactInfoModel, bool>> predicate, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        return Context.ListArtifactsAsync(predicate, cancellationToken);
    }

    /// <inheritdoc />
    public Task<List<ArtifactInfo>> ListArtifactsAsync(string tool, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        return Context.ListArtifactsAsync(tool, cancellationToken);
    }

    /// <inheritdoc />
    public Task<List<ArtifactInfo>> ListArtifactsAsync(string tool, string group, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        return Context.ListArtifactsAsync(tool, group, cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask AddArtifactAsync(ArtifactInfo artifactInfo, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        return Context.AddArtifactAsync(artifactInfo, cancellationToken);
    }

    /// <inheritdoc />
    public Task<List<ArtifactResourceInfo>> ListResourcesAsync(ArtifactKey key, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        return Context.ListResourcesAsync(key, cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask AddResourceAsync(ArtifactResourceInfo artifactResourceInfo, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        return Context.AddResourceAsync(artifactResourceInfo, cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask<ArtifactInfo?> TryGetArtifactAsync(ArtifactKey key, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        return Context.TryGetArtifactAsync(key, cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask<ArtifactResourceInfo?> TryGetResourceAsync(ArtifactResourceKey key, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        return Context.TryGetResourceAsync(key, cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask RemoveArtifactAsync(ArtifactKey key, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        return Context.RemoveArtifactAsync(key, cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask RemoveResourceAsync(ArtifactResourceKey key, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        return Context.RemoveResourceAsync(key, cancellationToken);
    }

    /// <summary>
    /// Saves changes.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        return Context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Context.Dispose();
        Context = null!;
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        await Context.DisposeAsync().ConfigureAwait(false);
        Context = null!;
        GC.SuppressFinalize(this);
    }

    private void EnsureNotDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
