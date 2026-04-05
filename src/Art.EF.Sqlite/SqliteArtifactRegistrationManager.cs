using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Art.EF.Sqlite;

/// <summary>
/// Represents an sqlite-backed artifact registration manager.
/// </summary>
public class SqliteArtifactRegistrationManager : EFArtifactRegistrationManager<SqliteArtifactContext>, IArtifactRegistrationManagerCleanup
{
    /// <summary>
    /// Creates a new instance of <see cref="SqliteArtifactRegistrationManager"/>.
    /// </summary>
    /// <remarks>
    /// Sqlite file backing if environment variable art_ef_sqlite_backing_file is set, otherwise in-memory Sqlite backing
    /// </remarks>
    public SqliteArtifactRegistrationManager() : base(new SqliteArtifactContextFactory().CreateDbContext([]))
    {
        SetupInternalWithAutoDisposeContextOnException(Context, SqliteArtifactRegistrationManagerConfig.Default with { IsReadOnly = Context.SqliteIsReadOnly, ApplyMigrationsOnStartup = !Context.SqliteIsReadOnly });
    }

    /// <summary>
    /// Creates a new instance of <see cref="SqliteArtifactRegistrationManager"/> with in-memory Sqlite backing.
    /// </summary>
    /// <param name="requireInMemory">If true, require using in-memory database, otherwise allow fallback to environment variable.</param>
    /// <param name="config">Configuration to use.</param>
    /// <remarks>
    /// Sqlite file backing if environment variable art_ef_sqlite_backing_file is set and <paramref name="requireInMemory"/> is false, otherwise in-memory Sqlite backing
    /// </remarks>
    public SqliteArtifactRegistrationManager(
        bool requireInMemory,
        SqliteArtifactRegistrationManagerConfig config
    )
        : base(new SqliteArtifactContextFactory(requireInMemory, config.IsReadOnly).CreateDbContext([]))
    {
        SetupInternalWithAutoDisposeContextOnException(Context, config);
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
        : base(new SqliteArtifactContextFactory(file, config.IsReadOnly).CreateDbContext([]))
    {
        SetupInternalWithAutoDisposeContextOnException(Context, config);
    }

    internal SqliteArtifactRegistrationManager(
        SqliteArtifactContext context,
        SqliteArtifactRegistrationManagerConfig config
    )
        : base(context)
    {
        SetupInternal(context, config);
    }

    private void SetupInternalWithAutoDisposeContextOnException(
        SqliteArtifactContext context,
        SqliteArtifactRegistrationManagerConfig config
    )
    {
        try
        {
            SetupInternal(context.SqliteIsReadOnly, context.SqliteUsingInMemory, config);
        }
        catch
        {
            context.Dispose();
            throw;
        }
    }

    private void SetupInternal(
        SqliteArtifactContext context,
        SqliteArtifactRegistrationManagerConfig config
    )
    {
        SetupInternal(context.SqliteIsReadOnly, context.SqliteUsingInMemory, config);
    }

    private void SetupInternal(
        bool instanceIsReadOnly,
        bool instanceIsInMemory,
        SqliteArtifactRegistrationManagerConfig config
    )
    {
        if (config.IsReadOnly && config.ApplyMigrationsOnStartup)
        {
            throw new ArgumentException($"Cannot combine {nameof(SqliteArtifactRegistrationManagerConfig.IsReadOnly)} and {nameof(SqliteArtifactRegistrationManagerConfig.ApplyMigrationsOnStartup)} on config");
        }
        if (config.IsReadOnly != instanceIsReadOnly)
        {
            throw new ArgumentException("Mismatch between readonly values of config and factory/instance");
        }
        if (instanceIsInMemory)
        {
            if (config.ApplyMigrationsOnStartup)
            {
                if (instanceIsReadOnly)
                {
                    throw new ArgumentException("Cannot apply migrations to read-only in-memory database");
                }
                EnsureCleanInMemoryReadWriteSetup(Context.Database);
            }
        }
        else
        {
            if (config.ApplyMigrationsOnStartup)
            {
                if (instanceIsReadOnly)
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
                        string sqliteFile;
                        try
                        {
                            var sqliteConn = (SqliteConnection)Context.Database.GetDbConnection();
                            sqliteConn.Open();
                            sqliteFile = sqliteConn.DataSource;
                        }
                        catch
                        {
                            sqliteFile = "<<unknown>>";
                        }
                        throw new EFPendingMigrationsPresentException(pendingMigrations,
                            config.IsReadOnly
                                ? $"The database file {sqliteFile} for this instance is mounted as read-only, but there are pending migrations that have not been applied: {pendingMigrationsStr}"
                                : $"This instance for database file {sqliteFile} is configured to not apply migrations on startup, but there are pending migrations that have not been applied: {pendingMigrationsStr}"
                        );
                    }
                }
            }
        }
    }

    private static void EnsureCleanInMemoryReadWriteSetup(DatabaseFacade dbFacade)
    {
        dbFacade.EnsureDeleted();
        dbFacade.EnsureCreated();
    }

    public Task CleanupDatabaseAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfReadOnly();
        return Context.CleanupDatabaseAsync(cancellationToken);
    }
}
