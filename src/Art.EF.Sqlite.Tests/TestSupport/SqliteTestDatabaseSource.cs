using System;
using System.IO;
using System.Reflection;
using Art.Common.IO;
using Art.EF.TestsBase;

namespace Art.EF.Sqlite.Tests.TestSupport;

public class SqliteTestDatabaseSource : IEFTestDatabaseSource
{
    private readonly string _databaseFile;

    public SqliteTestDatabaseSource()
    {
        _databaseFile = ArtIOUtility.CreateRandomPath(Path.GetTempPath(), ".db");
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
        var factory = new TestSqliteArtifactContextFactory(migrationsAssembly, _databaseFile, isReadonly: config.IsReadOnly);
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
