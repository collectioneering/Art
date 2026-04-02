using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Art.EF.Sqlite;

/// <summary>
/// Represents an sqlite-backed artifact registration manager.
/// </summary>
public class SqliteArtifactRegistrationManager : EFArtifactRegistrationManager<SqliteArtifactContext>, IArtifactRegistrationManagerCleanup
{
    internal SqliteArtifactRegistrationManager(
        SqliteArtifactContextFactory factory,
        SqliteArtifactRegistrationManagerConfig config) : base(factory)
    {
        if (config.IsReadOnly && config.ApplyMigrationsOnStartup)
        {
            throw new ArgumentException($"Cannot combine {nameof(SqliteArtifactRegistrationManagerConfig.IsReadOnly)} and {nameof(SqliteArtifactRegistrationManagerConfig.ApplyMigrationsOnStartup)} on config");
        }
        if (config.IsReadOnly != factory._isReadonly)
        {
            throw new ArgumentException("Mismatch between readonly values of config and factory");
        }
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
            if (config.ApplyMigrationsOnStartup)
            {
                if (factory._isReadonly)
                {
                    if (!config.DisablePendingMigrationsCheck)
                    {
                        var pendingMigrations = Context.Database.GetPendingMigrations().ToList();
                        if (pendingMigrations.Count > 0)
                        {
                            string pendingMigrationsStr = new StringBuilder().AppendJoin(", ", pendingMigrations).ToString();
                            throw new EFPendingMigrationsPresentException(pendingMigrations, $"There are pending migrations that cannot be applied to a read-only database: {pendingMigrationsStr}");
                        }
                    }
                }
                else
                {
                    Context.Database.Migrate();
                }
            }
            else
            {
                if (!config.DisablePendingMigrationsCheck)
                {
                    var pendingMigrations = Context.Database.GetPendingMigrations().ToList();
                    if (pendingMigrations.Count > 0)
                    {
                        string pendingMigrationsStr = new StringBuilder().AppendJoin(", ", pendingMigrations).ToString();
                        throw new EFPendingMigrationsPresentException(pendingMigrations, $"This instance is configured to not apply migrations on startup, but there are pending migrations that have not been applied: {pendingMigrationsStr}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Creates a new instance of <see cref="SqliteArtifactRegistrationManager"/>.
    /// </summary>
    /// <remarks>
    /// Sqlite file backing if environment variable art_ef_sqlite_backing_file is set, otherwise in-memory Sqlite backing
    /// </remarks>
    public SqliteArtifactRegistrationManager()
        : this(new SqliteArtifactContextFactory(), SqliteArtifactRegistrationManagerConfig.Default)
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="SqliteArtifactRegistrationManager"/> with in-memory Sqlite backing.
    /// </summary>
    /// <param name="inMemory">If true, use in-memory otherwise allow fallback to environment variable.</param>
    /// <param name="config">Configuration to use.</param>
    /// <remarks>
    /// Sqlite file backing if environment variable art_ef_sqlite_backing_file is set and <paramref name="inMemory"/> is false, otherwise in-memory Sqlite backing
    /// </remarks>
    public SqliteArtifactRegistrationManager(
        bool inMemory,
        SqliteArtifactRegistrationManagerConfig config)
        : this(new SqliteArtifactContextFactory(inMemory, config.IsReadOnly), config)
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="SqliteArtifactRegistrationManager"/> with the specified Sqlite backing file.
    /// </summary>
    /// <param name="file">Sqlite backing file.</param>
    /// <param name="config">Configuration to use.</param>
    public SqliteArtifactRegistrationManager(
        string file,
        SqliteArtifactRegistrationManagerConfig config
    )
        : this(new SqliteArtifactContextFactory(file, config.IsReadOnly), config)
    {
    }

    public Task CleanupDatabaseAsync(CancellationToken cancellationToken = default)
    {
        return Context.CleanupDatabaseAsync(cancellationToken);
    }
}
