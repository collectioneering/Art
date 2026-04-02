using System.Reflection;

namespace Art.EF.Sqlite.Tests.Migrations2;

/// <summary>
/// Factory for sqlite-backed context for artifact registration.
/// </summary>
public class TestMigrations2SqliteArtifactContextFactory : SqliteArtifactContextFactory
{
    /// <summary>
    /// Creates a new instance of <see cref="TestMigrations2SqliteArtifactContextFactory"/>.
    /// </summary>
    /// <remarks>
    /// Sqlite file backing if environment variable (by default, art_ef_sqlite_backing_file) is set, otherwise in-memory Sqlite backing
    /// <br/>
    /// </remarks>
    public TestMigrations2SqliteArtifactContextFactory()
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="TestMigrations2SqliteArtifactContextFactory"/> with in-memory Sqlite backing.
    /// </summary>
    /// <param name="inMemory">If true, use in-memory otherwise allow fallback to environment variable.</param>
    /// <param name="isReadonly">If true, writes to the database are disabled.</param>
    /// <remarks>
    /// Sqlite file backing if environment variable (by default, art_ef_sqlite_backing_file) is set and <paramref name="inMemory"/> is false, otherwise in-memory Sqlite backing
    /// </remarks>
    public TestMigrations2SqliteArtifactContextFactory(bool inMemory, bool isReadonly = false) : base(inMemory, isReadonly)
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="TestMigrations2SqliteArtifactContextFactory"/> with Sqlite file backing.
    /// </summary>
    /// <param name="storageFile">Path to sqlite storage file.</param>
    /// <param name="isReadonly">If true, writes to the database are disabled.</param>
    public TestMigrations2SqliteArtifactContextFactory(string storageFile, bool isReadonly = false) : base(storageFile, isReadonly)
    {
    }

    /// <inheritdoc/>
    public override Assembly MigrationAssembly => typeof(TestSqliteMigrations2).Assembly;
}
