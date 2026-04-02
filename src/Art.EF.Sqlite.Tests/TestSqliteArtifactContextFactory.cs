using System.Reflection;

namespace Art.EF.Sqlite.Tests;

/// <summary>
/// Factory for sqlite-backed context for artifact registration.
/// </summary>
public class TestSqliteArtifactContextFactory : SqliteArtifactContextFactory
{
    /// <summary>
    /// Creates a new instance of <see cref="TestSqliteArtifactContextFactory"/>.
    /// </summary>
    /// <param name="migrationsAssembly">Migrations assembly.</param>
    /// <remarks>
    /// Sqlite file backing if environment variable (by default, art_ef_sqlite_backing_file) is set, otherwise in-memory Sqlite backing
    /// <br/>
    /// </remarks>
    public TestSqliteArtifactContextFactory(Assembly migrationsAssembly)
    {
        _migrationsAssembly = migrationsAssembly;
    }

    /// <summary>
    /// Creates a new instance of <see cref="TestSqliteArtifactContextFactory"/> with in-memory Sqlite backing.
    /// </summary>
    /// <param name="migrationsAssembly">Migrations assembly.</param>
    /// <param name="inMemory">If true, use in-memory otherwise allow fallback to environment variable.</param>
    /// <param name="isReadonly">If true, writes to the database are disabled.</param>
    /// <remarks>
    /// Sqlite file backing if environment variable (by default, art_ef_sqlite_backing_file) is set and <paramref name="inMemory"/> is false, otherwise in-memory Sqlite backing
    /// </remarks>
    public TestSqliteArtifactContextFactory(Assembly migrationsAssembly, bool inMemory, bool isReadonly = false) : base(inMemory, isReadonly)
    {
        _migrationsAssembly = migrationsAssembly;
    }

    /// <summary>
    /// Creates a new instance of <see cref="TestSqliteArtifactContextFactory"/> with Sqlite file backing.
    /// </summary>
    /// <param name="migrationsAssembly">Migrations assembly.</param>
    /// <param name="storageFile">Path to sqlite storage file.</param>
    /// <param name="isReadonly">If true, writes to the database are disabled.</param>
    public TestSqliteArtifactContextFactory(Assembly migrationsAssembly, string storageFile, bool isReadonly = false) : base(storageFile, isReadonly)
    {
        _migrationsAssembly = migrationsAssembly;
    }

    /// <inheritdoc/>
    public override Assembly MigrationAssembly => _migrationsAssembly;

    private Assembly _migrationsAssembly;
}
