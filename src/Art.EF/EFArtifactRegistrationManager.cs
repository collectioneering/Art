using System.Linq.Expressions;
using System.Runtime.CompilerServices;

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
    /// Creates a new instance of <see cref="EFArtifactRegistrationManager{T}"/> using the <see cref="ArtifactContext"/>.
    /// </summary>
    /// <param name="context">Artifact context to wrap.</param>
    public EFArtifactRegistrationManager(TContext context)
    {
        Context = context;
    }

    /// <summary>
    /// Creates a new instance of <see cref="EFArtifactRegistrationManager{T}"/> using the specified factory.
    /// </summary>
    /// <param name="factory">Context factory.</param>
    public EFArtifactRegistrationManager(ArtifactContextFactoryBase<TContext> factory)
    {
        Context = factory.CreateDbContext([]);
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
        ThrowIfReadOnly();
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
        ThrowIfReadOnly();
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
        ThrowIfReadOnly();
        return Context.RemoveArtifactAsync(key, cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask RemoveResourceAsync(ArtifactResourceKey key, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        ThrowIfReadOnly();
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
        ThrowIfReadOnly();
        return Context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Throws an exception if the underlying context is read-only.
    /// </summary>
    /// <param name="callerMemberName"></param>
    /// <exception cref="InvalidOperationException"></exception>
    protected void ThrowIfReadOnly([CallerMemberName] string? callerMemberName = null)
    {
        if (Context.IsReadOnly)
        {
            throw new InvalidOperationException($"Cannot call {callerMemberName ?? "this member"} because the database context is read-only");
        }
    }

    private void EnsureNotDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }


    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes resources held by this instance.
    /// </summary>
    /// <param name="disposing">True if disposing, false if running through finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        _disposed = true;
        if (disposing)
        {
            var context = Context;
            if (ReferenceEquals(context, null))
            {
                return;
            }
            context.Dispose();
            Context = null!;
        }
    }

    /// <summary>
    /// Disposes resources held by this instance.
    /// </summary>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_disposed) return;
        _disposed = true;
        var context = Context;
        if (ReferenceEquals(context, null))
        {
            return;
        }
        await context.DisposeAsync().ConfigureAwait(false);
        Context = null!;
    }

    /// <summary>
    /// Cleans up instance data when finalizing.
    /// </summary>
    ~EFArtifactRegistrationManager()
    {
        Dispose(false);
    }
}
