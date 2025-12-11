using Microsoft.EntityFrameworkCore;

namespace Art.EF.Sqlite;

public class SqliteArtifactContext : ArtifactContext
{
    private bool _disposed;

    public SqliteArtifactContext(DbContextOptions<ArtifactContext> options) : base(options)
    {
    }

    public async Task CleanupDatabaseAsync(CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        if (!WaitGuard.WaitOne(0))
        {
            throw new InvalidOperationException($"Concurrent access to {nameof(ArtifactContext)} is disallowed");
        }
        try
        {
            await Database.ExecuteSqlAsync($"VACUUM;", cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            WaitGuard.Set();
        }
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        base.Dispose();
        if (_disposed) return;
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync().ConfigureAwait(false);
        if (_disposed) return;
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private void EnsureNotDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
