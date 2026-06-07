namespace Art.Common.Management;

/// <summary>
/// Represents a stream that will be committed upon disposal if <see cref="ICommittable.ShouldCommit"/> is set.
/// </summary>
public abstract class CommonCommittableStream : Stream
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
        CommitInternal(Committable?.ShouldCommit ?? false);
    }

    /// <inheritdoc />
    public override async ValueTask DisposeAsync()
    {
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
}
