using System;
using System.IO;
using System.Reflection;
using Art.Common.IO;
using Art.EF.TestsBase;

namespace Art.EF.Sqlite.Tests.TestSupport;

public class SqliteTestDatabaseSource : IEFTestDatabaseSource
{
    public static string CreateFileDb()
    {
        string tmpParentDirectory = Path.Join(Path.GetTempPath(), "collectioneering_art_test_db");
        Directory.CreateDirectory(tmpParentDirectory);
        return ArtIOUtility.CreateRandomPath(tmpParentDirectory, ".db");
    }

    private readonly string _databaseFile;

    public SqliteTestDatabaseSource()
    {
        _databaseFile = CreateFileDb();
    }

    public IArtifactRegistrationManager CreateArtifactRegistrationManager(TestDatabaseConfig config, Assembly? migrationsAssembly)
    {
        var sqliteArtifactRegistrationManagerConfig = new SqliteArtifactRegistrationManagerConfig(
            ApplyMigrationsOnStartup: config.ApplyMigrationsOnStartup,
            IsReadOnly: config.IsReadOnly,
            DisablePendingMigrationsCheck: config.DisablePendingMigrationsCheck);
        if (migrationsAssembly == null)
        {
            return new TestSqliteArtifactRegistrationManager(_databaseFile, sqliteArtifactRegistrationManagerConfig);
        }
        var factory = new TestSqliteArtifactContextFactory(migrationsAssembly, _databaseFile, isReadOnly: config.IsReadOnly);
        return new TestSqliteArtifactRegistrationManager(factory, sqliteArtifactRegistrationManagerConfig);
    }

    private void ReleaseUnmanagedResources()
    {
        File.Delete(_databaseFile);
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~SqliteTestDatabaseSource() => ReleaseUnmanagedResources();
}
