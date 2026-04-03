using System.Reflection;

namespace Art.EF.Sqlite.Tests.Migrations1;

/// <summary>
/// Factory for sqlite-backed context for artifact registration.
/// </summary>
public class TestMigrations1SqliteArtifactContextFactory : SqliteArtifactContextFactory
{
    /// <summary>
    /// Creates a new instance of <see cref="TestMigrations1SqliteArtifactContextFactory"/>.
    /// </summary>
    /// <remarks>
    /// Sqlite file backing if environment variable (by default, art_ef_sqlite_backing_file) is set, otherwise in-memory Sqlite backing
    /// <br/>
    /// </remarks>
    public TestMigrations1SqliteArtifactContextFactory()
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="TestMigrations1SqliteArtifactContextFactory"/> with in-memory Sqlite backing.
    /// </summary>
    /// <param name="requireInMemory">If true, require using in-memory database, otherwise allow fallback to environment variable.</param>
    /// <param name="isReadonly">If true, writes to the database are disabled.</param>
    /// <remarks>
    /// Sqlite file backing if environment variable (by default, art_ef_sqlite_backing_file) is set and <paramref name="requireInMemory"/> is false, otherwise in-memory Sqlite backing
    /// </remarks>
    public TestMigrations1SqliteArtifactContextFactory(
        bool requireInMemory,
        bool isReadonly = false
        )
        : base(requireInMemory, isReadonly)
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="TestMigrations1SqliteArtifactContextFactory"/> with Sqlite file backing.
    /// </summary>
    /// <param name="storageFile">Path to sqlite storage file.</param>
    /// <param name="isReadonly">If true, writes to the database are disabled.</param>
    public TestMigrations1SqliteArtifactContextFactory(string storageFile, bool isReadonly = false) : base(storageFile, isReadonly)
    {
    }

    /// <inheritdoc/>
    public override Assembly MigrationAssembly => typeof(TestSqliteMigrations1).Assembly;
}
