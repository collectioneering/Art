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
    public SqliteArtifactContextFactory() : this(false, isReadOnly: false)
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="SqliteArtifactContextFactory"/> with in-memory Sqlite backing.
    /// </summary>
    /// <param name="requireInMemory">If true, require using in-memory database, otherwise allow fallback to environment variable.</param>
    /// <param name="isReadOnly">If true, writes to the database are disabled.</param>
    /// <remarks>
    /// Sqlite file backing if environment variable (by default, art_ef_sqlite_backing_file) is set and <paramref name="requireInMemory"/> is false, otherwise in-memory Sqlite backing
    /// </remarks>
    public SqliteArtifactContextFactory(bool requireInMemory, bool isReadOnly = false)
    {
        _requireInMemory = requireInMemory;
        _isReadOnly = isReadOnly;
    }

    /// <summary>
    /// Creates a new instance of <see cref="SqliteArtifactContextFactory"/> with Sqlite file backing.
    /// </summary>
    /// <param name="storageFile">Path to sqlite storage file.</param>
    /// <param name="isReadOnly">If true, writes to the database are disabled.</param>
    public SqliteArtifactContextFactory(string storageFile, bool isReadOnly = false)
    {
        StorageFile = storageFile;
        _isReadOnly = isReadOnly;
    }

    internal readonly bool _requireInMemory;
    internal readonly bool _isReadOnly;

    internal bool UsingInMemory;

    internal static string CreateMemoryFileId() => $"{Guid.NewGuid():N}";

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

    internal string? MemoryFileId { get; set; }

    internal string BuildConnectionString()
    {
        string? file = StorageFile ?? (_requireInMemory ? null : Environment.GetEnvironmentVariable(EnvStorageFile));
        var queryStringParts = new List<string>();
        if (file == null)
        {
            UsingInMemory = true;
            file = MemoryFileId ??= CreateMemoryFileId();
            queryStringParts.Add("mode=memory");
            queryStringParts.Add("cache=shared");
        }
        else if (_isReadOnly)
        {
            queryStringParts.Add("immutable=1");
            queryStringParts.Add("mode=ro");
        }
        var connectionStringBuilder = new StringBuilder("DataSource=");
        connectionStringBuilder.Append("file:");
        connectionStringBuilder.Append(file);
        if (queryStringParts.Count > 0)
        {
            connectionStringBuilder.Append('?');
            connectionStringBuilder.AppendJoin("&", queryStringParts);
        }
        connectionStringBuilder.Append(';');
        if (_isReadOnly)
        {
            connectionStringBuilder.Append("Mode=ReadOnly;");
        }
        return connectionStringBuilder.ToString();
    }

    /// <inheritdoc />
    public override SqliteArtifactContext CreateDbContext(string[] args)
    {
        var ob = new DbContextOptionsBuilder<ArtifactContext>();
        ob.UseSqlite(BuildConnectionString(), b => b.MigrationsAssembly(MigrationAssembly.FullName));
        var context = new SqliteArtifactContext(ob.Options);
        context.SqliteUsingInMemory = UsingInMemory;
        context.SqliteIsReadOnly = _isReadOnly;
        return context;
    }
}
