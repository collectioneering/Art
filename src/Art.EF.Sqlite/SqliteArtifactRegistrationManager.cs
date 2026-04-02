using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Art.EF.Sqlite;

/// <summary>
/// Represents an sqlite-backed artifact registration manager.
/// </summary>
public class SqliteArtifactRegistrationManager : EFArtifactRegistrationManager<SqliteArtifactContext>, IArtifactRegistrationManagerCleanup
{
    private SqliteArtifactRegistrationManager(SqliteArtifactContextFactory factory) : base(factory)
    {
        if (factory.UsingInMemory)
        {
            if (!factory._isReadonly)
            {
                Context.Database.EnsureDeleted();
                Context.Database.EnsureCreated();
            }
        }
        else
        {
            if (factory._isReadonly)
            {
                var pendingMigrations = Context.Database.GetPendingMigrations().ToList();
                if (pendingMigrations.Count > 0)
                {
                    string pendingMigrationsStr = new StringBuilder().AppendJoin(", ", pendingMigrations).ToString();
                    throw new InvalidOperationException($"There are pending migrations that cannot be applied to a read-only database: {pendingMigrationsStr}");
                }
            }
            else
            {
                Context.Database.Migrate();
            }
        }
    }

    /// <summary>
    /// Creates a new instance of <see cref="SqliteArtifactRegistrationManager"/>.
    /// </summary>
    /// <remarks>
    /// Sqlite file backing if environment variable art_ef_sqlite_backing_file is set, otherwise in-memory Sqlite backing
    /// </remarks>
    public SqliteArtifactRegistrationManager() : this(new SqliteArtifactContextFactory())
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="SqliteArtifactRegistrationManager"/> with in-memory Sqlite backing.
    /// </summary>
    /// <param name="inMemory">If true, use in-memory otherwise allow fallback to environment variable.</param>
    /// <param name="isReadonly">If true, writes to the database are disabled.</param>
    /// <remarks>
    /// Sqlite file backing if environment variable art_ef_sqlite_backing_file is set and <paramref name="inMemory"/> is false, otherwise in-memory Sqlite backing
    /// </remarks>
    public SqliteArtifactRegistrationManager(bool inMemory, bool isReadonly = false) : this(new SqliteArtifactContextFactory(inMemory, isReadonly))
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="SqliteArtifactRegistrationManager"/> with the specified Sqlite backing file.
    /// </summary>
    /// <param name="file">Sqlite backing file.</param>
    /// <param name="isReadonly">If true, writes to the database are disabled.</param>
    public SqliteArtifactRegistrationManager(string file, bool isReadonly = false) : this(new SqliteArtifactContextFactory(file, isReadonly))
    {
    }

    public Task CleanupDatabaseAsync(CancellationToken cancellationToken = default)
    {
        return Context.CleanupDatabaseAsync(cancellationToken);
    }
}
