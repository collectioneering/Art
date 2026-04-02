using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Art.Common.IO;
using Art.EF.Sqlite.Tests.Migrations1;
using Art.EF.Sqlite.Tests.Migrations2;
using Microsoft.Data.Sqlite;

namespace Art.EF.Sqlite.Tests;

public class SqliteTests_Migrations : SqliteTestsBase
{
    [Fact]
    public async Task ReadWrite_UnappliedInitialMigration_Fail()
    {
        string tempFile = ArtIOUtility.CreateRandomPath(Path.GetTempPath(), ".db");
        try
        {
            try
            {
                var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: false, IsReadOnly: false, DisablePendingMigrationsCheck: false);
                var factory = new TestSqliteArtifactContextFactory(typeof(TestSqliteMigrations1).Assembly, tempFile, isReadonly: false);
                var pendingMigrationsPresentException = Assert.Throws<EFPendingMigrationsPresentException>(() => new SqliteArtifactRegistrationManager(factory, config));
                Assert.Single(pendingMigrationsPresentException.PendingMigrations);
                Assert.EndsWith("_InitialCreate", pendingMigrationsPresentException.PendingMigrations.First());
            }
            finally
            {
                SqliteConnection.ClearAllPools();
            }
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ReadOnly_UnappliedInitialMigration_Fail()
    {
        string tempFile = ArtIOUtility.CreateRandomPath(Path.GetTempPath(), ".db");
        try
        {
            try
            {
                var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: false, IsReadOnly: false, DisablePendingMigrationsCheck: false);
                var factory = new TestSqliteArtifactContextFactory(typeof(TestSqliteMigrations1).Assembly, tempFile, isReadonly: false);
                var pendingMigrationsPresentException = Assert.Throws<EFPendingMigrationsPresentException>(() => new SqliteArtifactRegistrationManager(factory, config));
                Assert.Single(pendingMigrationsPresentException.PendingMigrations);
                Assert.EndsWith("_InitialCreate", pendingMigrationsPresentException.PendingMigrations.First());
            }
            finally
            {
                SqliteConnection.ClearAllPools();
            }
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ReadWrite_AppliedInitialMigration_Success()
    {
        string tempFile = ArtIOUtility.CreateRandomPath(Path.GetTempPath(), ".db");
        try
        {
            try
            {
                var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
                var factory = new TestSqliteArtifactContextFactory(typeof(TestSqliteMigrations1).Assembly, tempFile, isReadonly: false);
                using SqliteArtifactRegistrationManager r = new(factory, config);
                Assert.False(r.Context.UsingInMemory);
                await TestSqliteDatabase(r);
            }
            finally
            {
                SqliteConnection.ClearAllPools();
            }
            try
            {
                var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
                var factory = new TestSqliteArtifactContextFactory(typeof(TestSqliteMigrations1).Assembly, tempFile, isReadonly: false);
                using SqliteArtifactRegistrationManager r = new(factory, config);
                Assert.False(r.Context.UsingInMemory);
                await VerifyWrittenSqliteDatabase(r);
            }
            finally
            {
                SqliteConnection.ClearAllPools();
            }
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ReadOnly_AppliedInitialMigration_Success()
    {
        string tempFile = ArtIOUtility.CreateRandomPath(Path.GetTempPath(), ".db");
        try
        {
            try
            {
                var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
                var factory = new TestSqliteArtifactContextFactory(typeof(TestSqliteMigrations1).Assembly, tempFile, isReadonly: false);
                using SqliteArtifactRegistrationManager r = new(factory, config);
                Assert.False(r.Context.UsingInMemory);
                await TestSqliteDatabase(r);
            }
            finally
            {
                SqliteConnection.ClearAllPools();
            }
            try
            {
                var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: false, IsReadOnly: true, DisablePendingMigrationsCheck: false);
                var factory = new TestSqliteArtifactContextFactory(typeof(TestSqliteMigrations1).Assembly, tempFile, isReadonly: true);
                using SqliteArtifactRegistrationManager r = new(factory, config);
                Assert.False(r.Context.UsingInMemory);
                await VerifyWrittenSqliteDatabase(r);
            }
            finally
            {
                SqliteConnection.ClearAllPools();
            }
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ReadWrite_AppliedInitialMigration_PendingOtherMigration_Fail()
    {
        string tempFile = ArtIOUtility.CreateRandomPath(Path.GetTempPath(), ".db");
        try
        {
            try
            {
                var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
                var factory = new TestSqliteArtifactContextFactory(typeof(TestSqliteMigrations1).Assembly, tempFile, isReadonly: false);
                using SqliteArtifactRegistrationManager r = new(factory, config);
                Assert.False(r.Context.UsingInMemory);
                await TestSqliteDatabase(r);
            }
            finally
            {
                SqliteConnection.ClearAllPools();
            }
            try
            {
                var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: false, IsReadOnly: true, DisablePendingMigrationsCheck: false);
                var factory = new TestSqliteArtifactContextFactory(typeof(TestSqliteMigrations2).Assembly, tempFile, isReadonly: true);
                var pendingMigrationsPresentException = Assert.Throws<EFPendingMigrationsPresentException>(() => new SqliteArtifactRegistrationManager(factory, config));
                Assert.Single(pendingMigrationsPresentException.PendingMigrations);
                Assert.EndsWith("_DummyMigrationQX1", pendingMigrationsPresentException.PendingMigrations.First());
            }
            finally
            {
                SqliteConnection.ClearAllPools();
            }
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ReadOnly_AppliedInitialMigration_PendingOtherMigration_Fail()
    {
        string tempFile = ArtIOUtility.CreateRandomPath(Path.GetTempPath(), ".db");
        try
        {
            try
            {
                var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
                var factory = new TestSqliteArtifactContextFactory(typeof(TestSqliteMigrations1).Assembly, tempFile, isReadonly: false);
                using SqliteArtifactRegistrationManager r = new(factory, config);
                Assert.False(r.Context.UsingInMemory);
                await TestSqliteDatabase(r);
            }
            finally
            {
                SqliteConnection.ClearAllPools();
            }
            try
            {
                var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: false, IsReadOnly: true, DisablePendingMigrationsCheck: false);
                var factory = new TestSqliteArtifactContextFactory(typeof(TestSqliteMigrations2).Assembly, tempFile, isReadonly: true);
                var pendingMigrationsPresentException = Assert.Throws<EFPendingMigrationsPresentException>(() => new SqliteArtifactRegistrationManager(factory, config));
                Assert.Single(pendingMigrationsPresentException.PendingMigrations);
                Assert.EndsWith("_DummyMigrationQX1", pendingMigrationsPresentException.PendingMigrations.First());
            }
            finally
            {
                SqliteConnection.ClearAllPools();
            }
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ReadWrite_AppliedInitialMigration_AppliedOtherMigration_Success()
    {
        string tempFile = ArtIOUtility.CreateRandomPath(Path.GetTempPath(), ".db");
        try
        {
            try
            {
                var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
                var factory = new TestSqliteArtifactContextFactory(typeof(TestSqliteMigrations1).Assembly, tempFile, isReadonly: false);
                using SqliteArtifactRegistrationManager r = new(factory, config);
                Assert.False(r.Context.UsingInMemory);
                await TestSqliteDatabase(r);
            }
            finally
            {
                SqliteConnection.ClearAllPools();
            }
            try
            {
                var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
                var factory = new TestSqliteArtifactContextFactory(typeof(TestSqliteMigrations2).Assembly, tempFile, isReadonly: false);
                using SqliteArtifactRegistrationManager r = new(factory, config);
                Assert.False(r.Context.UsingInMemory);
                await VerifyWrittenSqliteDatabase(r);
            }
            finally
            {
                SqliteConnection.ClearAllPools();
            }
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(tempFile);
        }
    }
}
