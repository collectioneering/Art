using Art.Common.IO;

namespace Art.Common.Management;

/// <summary>
/// Base type for <see cref="DelegatingStream"/> implementations that support <see cref="ICommittable"/> to manage committing data.
/// </summary>
public abstract class CommittableDelegatingStream : DelegatingStream
{
    /// <summary>
    /// Instance of <see cref="Committable"/> to use.
    /// </summary>
    public ICommittable? Committable { get; init; }

    /// <summary>
    /// If true, this stream has been committed.
    /// </summary>
    protected bool Committed { get; private set; }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            InnerStream.Dispose();
            CommitInternal(Committable?.ShouldCommit ?? false);
        }
    }

    /// <inheritdoc />
    public override async ValueTask DisposeAsync()
    {
        await InnerStream.DisposeAsync().ConfigureAwait(false);
        await CommitInternalAsync(Committable?.ShouldCommit ?? false).ConfigureAwait(false);
    }

    private void CommitInternal(bool shouldCommit)
    {
        if (Committed)
        {
            return;
        }
        Committed = true;
        Commit(shouldCommit);
    }

    private async ValueTask CommitInternalAsync(bool shouldCommit)
    {
        if (Committed)
        {
            return;
        }
        Committed = true;
        await CommitAsync(shouldCommit).ConfigureAwait(false);
    }

    /// <summary>
    /// Ensures this instance has not yet been committed.
    /// </summary>
    /// <exception cref="InvalidOperationException">Stream has been committed.</exception>
    protected void EnsureNotCommitted()
    {
        if (Committed)
        {
            throw new InvalidOperationException("Stream has already been committed");
        }
    }

    /// <summary>
    /// Performs data commit.
    /// </summary>
    /// <param name="shouldCommit">If true, perform commit. Otherwise, perform appropriate cleanup.</param>
    protected abstract void Commit(bool shouldCommit);

    /// <summary>
    /// Performs data commit.
    /// </summary>
    /// <param name="shouldCommit">If true, perform commit. Otherwise, perform appropriate cleanup.</param>
    protected virtual ValueTask CommitAsync(bool shouldCommit)
    {
        Commit(shouldCommit);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    protected override void ValidateSeekState()
    {
        EnsureNotCommitted();
    }

    /// <inheritdoc />
    protected override void ValidateFlushState()
    {
        EnsureNotCommitted();
    }

    /// <inheritdoc />
    protected override void ValidateSetLengthState()
    {
        EnsureNotCommitted();
    }

    /// <inheritdoc />
    protected override void ValidateReadState()
    {
        EnsureNotCommitted();
    }

    /// <inheritdoc />
    protected override void ValidateWriteState()
    {
        EnsureNotCommitted();
    }
}
