using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Art.EF.Sqlite;

/// <summary>
/// Factory for sqlite-backed context for artifact registration.
/// </summary>
public class SqliteArtifactContextFactory : ArtifactContextFactoryBase<SqliteArtifactContext>
{
    /// <summary>
    /// Creates a new instance of <see cref="SqliteArtifactContextFactory"/>.
    /// </summary>
    /// <remarks>
    /// Sqlite file backing if environment variable (by default, art_ef_sqlite_backing_file) is set, otherwise in-memory Sqlite backing
    /// <br/>
    /// </remarks>
    public SqliteArtifactContextFactory() : this(false, isReadonly: false)
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="SqliteArtifactContextFactory"/> with in-memory Sqlite backing.
    /// </summary>
    /// <param name="inMemory">If true, use in-memory otherwise allow fallback to environment variable.</param>
    /// <param name="isReadonly">If true, writes to the database are disabled.</param>
    /// <remarks>
    /// Sqlite file backing if environment variable (by default, art_ef_sqlite_backing_file) is set and <paramref name="inMemory"/> is false, otherwise in-memory Sqlite backing
    /// </remarks>
    public SqliteArtifactContextFactory(bool inMemory, bool isReadonly = false)
    {
        _inMemory = inMemory;
        _isReadonly = isReadonly;
    }

    /// <summary>
    /// Creates a new instance of <see cref="SqliteArtifactContextFactory"/> with Sqlite file backing.
    /// </summary>
    /// <param name="storageFile">Path to sqlite storage file.</param>
    /// <param name="isReadonly">If true, writes to the database are disabled.</param>
    public SqliteArtifactContextFactory(string storageFile, bool isReadonly = false)
    {
        StorageFile = storageFile;
        _isReadonly = isReadonly;
    }

    private readonly bool _inMemory;
    internal readonly bool _isReadonly;

    internal bool UsingInMemory;

    private const string Memory = "file::memory:?cache=shared";

    /// <summary>
    /// Environment variable for path to sqlite storage file.
    /// </summary>
    public virtual string EnvStorageFile { get; set; } = "art_ef_sqlite_backing_file";

    /// <summary>
    /// Path to sqlite storage file.
    /// </summary>
    public string? StorageFile { get; set; }

    /// <summary>
    /// Assembly with migrations for the database
    /// </summary>
    public virtual Assembly MigrationAssembly => GetType().Assembly;

    /// <inheritdoc />
    public override SqliteArtifactContext CreateDbContext(string[] args)
    {
        string? file = StorageFile ?? (_inMemory ? null : Environment.GetEnvironmentVariable(EnvStorageFile));
        if (file == null)
        {
            UsingInMemory = true;
            file = Memory;
        }
        var sb = new StringBuilder("DataSource=");
        sb.Append(file);
        sb.Append(';');
        if (_isReadonly)
        {
            sb.Append("Mode=ReadOnly;");
        }
        var ob = new DbContextOptionsBuilder<ArtifactContext>();
        ob.UseSqlite(sb.ToString(), b => b.MigrationsAssembly(MigrationAssembly.FullName));
        var context = new SqliteArtifactContext(ob.Options);
        context.UsingInMemory = UsingInMemory;
        context.SqliteIsReadOnly = _isReadonly;
        return context;
    }
}
