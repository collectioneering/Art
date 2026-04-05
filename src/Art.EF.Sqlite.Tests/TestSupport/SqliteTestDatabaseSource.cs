using System;
using System.IO;
using System.Reflection;
using Art.Common.IO;
using Art.EF.TestsBase;
using Microsoft.Data.Sqlite;

namespace Art.EF.Sqlite.Tests.TestSupport;

public sealed class SqliteTestDatabaseSource : IEFTestDatabaseSource
{
    private bool _disposed;

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

    public IArtifactRegistrationManager CreateArtifactRegistrationManagerWithThrowCleanup(TestDatabaseConfig config, Assembly? migrationsAssembly)
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
        try
        {
            return new TestSqliteArtifactRegistrationManager(factory, sqliteArtifactRegistrationManagerConfig);
        }
        catch
        {
            SqliteConnection.ClearPool(new SqliteConnection(factory.BuildConnectionString()));
            throw;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        File.Delete(_databaseFile);
    }
}
