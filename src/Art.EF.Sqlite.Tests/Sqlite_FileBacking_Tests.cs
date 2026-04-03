using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Art.Common.IO;
using Art.EF.TestsBase;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Art.EF.Sqlite.Tests;

public class Sqlite_FileBacking_Tests : EFTestsBase
{
    [Fact]
    public async Task FileBacking_ReadWrite_Success()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        string tempFile = ArtIOUtility.CreateRandomPath(Path.GetTempPath(), ".db");
        SqliteConnection? connection = null;
        try
        {
            var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
            using SqliteArtifactRegistrationManager r = new(tempFile, config);
            connection = r.Context.Database.GetDbConnection() as SqliteConnection;
            Assert.False(r.Context.UsingInMemory);
            await TestReadWriteDatabase(r, testCancellationToken);
        }
        finally
        {
            if (connection != null)
            {
                SqliteConnection.ClearPool(connection);
            }
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task FileBacking_ReadOnly_Succeeds()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        string tempFile = ArtIOUtility.CreateRandomPath(Path.GetTempPath(), ".db");
        SqliteConnection? connection = null;
        try
        {
            try
            {
                var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
                using SqliteArtifactRegistrationManager r = new(tempFile, config);
                connection = r.Context.Database.GetDbConnection() as SqliteConnection;
                Assert.False(r.Context.UsingInMemory);
                await PrepareDatabaseForReadOnly(r, testCancellationToken);
            }
            finally
            {
                if (connection != null)
                {
                    SqliteConnection.ClearPool(connection);
                    connection = null;
                }
            }

            try
            {
                var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: false, IsReadOnly: true, DisablePendingMigrationsCheck: false);
                using SqliteArtifactRegistrationManager r = new(tempFile, config);
                connection = r.Context.Database.GetDbConnection() as SqliteConnection;
                Assert.False(r.Context.UsingInMemory);
                await TestDatabaseReadOnly(r, testEmpty: false, testCancellationToken);
            }
            finally
            {
                if (connection != null)
                {
                    SqliteConnection.ClearPool(connection);
                }
            }
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task Vacuum_Shrinks()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        string tempFile = ArtIOUtility.CreateRandomPath(Path.GetTempPath(), ".db");
        const int n = 64;
        const int denominator = 4;
        string testValue = new('x', 2048);
        SqliteConnection? connection = null;
        try
        {
            // create base (large) set
            try
            {
                var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
                using SqliteArtifactRegistrationManager r = new(tempFile, config);
                connection = r.Context.Database.GetDbConnection() as SqliteConnection;
                Assert.False(r.Context.UsingInMemory);
                for (int i = 0; i < n; i++)
                {
                    await r.AddArtifactAsync(new ArtifactInfo(new ArtifactKey("TOOL", "GROUP", $"{testValue}{i}")), testCancellationToken);
                }
            }
            finally
            {
                if (connection != null)
                {
                    SqliteConnection.ClearPool(connection);
                    connection = null;
                }
            }
            // remove large number of entries
            try
            {
                var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
                using SqliteArtifactRegistrationManager r = new(tempFile, config);
                connection = r.Context.Database.GetDbConnection() as SqliteConnection;
                Assert.False(r.Context.UsingInMemory);
                foreach (var artifactInfo in (await r.ListArtifactsAsync(testCancellationToken)).ToList().Take(n - n / denominator))
                {
                    await r.RemoveArtifactAsync(artifactInfo.Key, testCancellationToken);
                }
            }
            finally
            {
                if (connection != null)
                {
                    SqliteConnection.ClearPool(connection);
                    connection = null;
                }
            }
            // get stable file size
            try
            {
                var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
                using SqliteArtifactRegistrationManager r = new(tempFile, config);
                connection = r.Context.Database.GetDbConnection() as SqliteConnection;
                Assert.False(r.Context.UsingInMemory);
            }
            finally
            {
                if (connection != null)
                {
                    SqliteConnection.ClearPool(connection);
                    connection = null;
                }
            }
            long fileSizeC = new FileInfo(tempFile).Length;
            // cleanup and measure
            try
            {
                var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
                using SqliteArtifactRegistrationManager r = new(tempFile, config);
                connection = r.Context.Database.GetDbConnection() as SqliteConnection;
                Assert.False(r.Context.UsingInMemory);
                await r.CleanupDatabaseAsync(testCancellationToken);
            }
            finally
            {
                if (connection != null)
                {
                    SqliteConnection.ClearPool(connection);
                }
            }
            long fileSizeD = new FileInfo(tempFile).Length;
            Assert.True(fileSizeD < fileSizeC);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
