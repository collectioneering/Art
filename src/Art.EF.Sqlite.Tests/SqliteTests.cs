using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Art.Common.IO;
using Art.EF.Sqlite.Tests.Migrations1;
using Art.EF.Sqlite.Tests.Migrations2;
using Microsoft.Data.Sqlite;

namespace Art.EF.Sqlite.Tests;

public class SqliteTests : SqliteTestsBase
{
    [Fact]
    public async Task TestSqliteDatabaseFile()
    {
        string tempFile = ArtIOUtility.CreateRandomPath(Path.GetTempPath(), ".db");
        try
        {
            var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
            using SqliteArtifactRegistrationManager r = new(tempFile, config);
            Assert.False(r.Context.UsingInMemory);
            await TestSqliteDatabase(r);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task TestSqliteDatabaseMemoryAsFallback()
    {
        try
        {
            var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
            await using SqliteArtifactRegistrationManager r = new(false, config);
            Assert.True(r.Context.UsingInMemory);
            await TestSqliteDatabase(r);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
        }
    }

    [Fact]
    public async Task TestSqliteDatabaseMemoryExplicit()
    {
        try
        {
            var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: false, IsReadOnly: false, DisablePendingMigrationsCheck: false);
            await using SqliteArtifactRegistrationManager r = new(true, config);
            Assert.True(r.Context.UsingInMemory);
            await TestSqliteDatabase(r);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
        }
    }

    [Fact]
    public async Task TestSqliteDatabaseFileReadOnly()
    {
        string tempFile = ArtIOUtility.CreateRandomPath(Path.GetTempPath(), ".db");
        try
        {
            try
            {
                var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
                using SqliteArtifactRegistrationManager r = new(tempFile, config);
                Assert.False(r.Context.UsingInMemory);
                await PrepareSqliteDatabaseForReadOnly(r);
            }
            finally
            {
                SqliteConnection.ClearAllPools();
            }

            try
            {
                var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: false, IsReadOnly: true, DisablePendingMigrationsCheck: false);
                using SqliteArtifactRegistrationManager r = new(tempFile, config);
                Assert.False(r.Context.UsingInMemory);
                await TestSqliteDatabaseReadOnly(r, testEmpty: false);
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
    public async Task TestSqliteDatabaseMemoryAsFallbackReadOnly()
    {
        try
        {
            var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: false, IsReadOnly: true, DisablePendingMigrationsCheck: false);
            await using SqliteArtifactRegistrationManager r = new(false, config);
            Assert.True(r.Context.UsingInMemory);
            await TestSqliteDatabaseReadOnly(r, testEmpty: true);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
        }
    }

    [Fact]
    public async Task TestSqliteDatabaseMemoryExplicitReadOnly()
    {
        try
        {
            var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: false, IsReadOnly: true, DisablePendingMigrationsCheck: false);
            await using SqliteArtifactRegistrationManager r = new(true, config);
            Assert.True(r.Context.UsingInMemory);
            await TestSqliteDatabaseReadOnly(r, testEmpty: true);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
        }
    }

    [Fact]
    public async Task TestSqliteDatabaseFileVacuum()
    {
        string tempFile = ArtIOUtility.CreateRandomPath(Path.GetTempPath(), ".db");
        const int n = 64;
        const int denominator = 4;
        string testValue = new('x', 2048);
        try
        {
            // create base (large) set
            try
            {
                var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
                using SqliteArtifactRegistrationManager r = new(tempFile, config);
                Assert.False(r.Context.UsingInMemory);
                for (int i = 0; i < n; i++)
                {
                    await r.AddArtifactAsync(new ArtifactInfo(new ArtifactKey("TOOL", "GROUP", $"{testValue}{i}")));
                }
            }
            finally
            {
                SqliteConnection.ClearAllPools();
            }
            // remove large number of entries
            try
            {
                var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
                using SqliteArtifactRegistrationManager r = new(tempFile, config);
                Assert.False(r.Context.UsingInMemory);
                foreach (var artifactInfo in (await r.ListArtifactsAsync()).ToList().Take(n - n / denominator))
                {
                    await r.RemoveArtifactAsync(artifactInfo.Key);
                }
            }
            finally
            {
                SqliteConnection.ClearAllPools();
            }
            // get stable file size
            try
            {
                var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
                using SqliteArtifactRegistrationManager r = new(tempFile, config);
                Assert.False(r.Context.UsingInMemory);
            }
            finally
            {
                SqliteConnection.ClearAllPools();
            }
            long fileSizeC = new FileInfo(tempFile).Length;
            // cleanup and measure
            try
            {
                var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
                using SqliteArtifactRegistrationManager r = new(tempFile, config);
                Assert.False(r.Context.UsingInMemory);
                await r.CleanupDatabaseAsync();
            }
            finally
            {
                SqliteConnection.ClearAllPools();
            }
            long fileSizeD = new FileInfo(tempFile).Length;
            Assert.True(fileSizeD < fileSizeC);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(tempFile);
        }
    }
}
