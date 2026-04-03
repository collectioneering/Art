using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Art.Common.IO;
using Art.EF.Sqlite.Tests.TestSupport;
using Art.EF.TestsBase;

namespace Art.EF.Sqlite.Tests;

public class Sqlite_FileBacking_Tests : EFTestsBase
{
    [Fact]
    public async Task FileBacking_ReadWrite_Success()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        string tempFile = ArtIOUtility.CreateRandomPath(Path.GetTempPath(), ".db");
        try
        {
            var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
            using TestSqliteArtifactRegistrationManager r = new(tempFile, config);
            Assert.False(r.Context.UsingInMemory);
            await TestReadWriteDatabase(r, testCancellationToken);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task FileBacking_ReadOnly_Succeeds()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        string tempFile = ArtIOUtility.CreateRandomPath(Path.GetTempPath(), ".db");
        try
        {
            {
                var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
                using TestSqliteArtifactRegistrationManager r = new(tempFile, config);
                Assert.False(r.Context.UsingInMemory);
                await PrepareDatabaseForReadOnly(r, testCancellationToken);
            }

            {
                var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: false, IsReadOnly: true, DisablePendingMigrationsCheck: false);
                using TestSqliteArtifactRegistrationManager r = new(tempFile, config);
                Assert.False(r.Context.UsingInMemory);
                await TestDatabaseReadOnly(r, testEmpty: false, testCancellationToken);
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
        try
        {
            // create base (large) set
            {
                var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
                using SqliteArtifactRegistrationManager r = new(tempFile, config);
                Assert.False(r.Context.UsingInMemory);
                for (int i = 0; i < n; i++)
                {
                    await r.AddArtifactAsync(new ArtifactInfo(new ArtifactKey("TOOL", "GROUP", $"{testValue}{i}")), testCancellationToken);
                }
            }
            // remove large number of entries
            {
                var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
                using TestSqliteArtifactRegistrationManager r = new(tempFile, config);
                Assert.False(r.Context.UsingInMemory);
                foreach (var artifactInfo in (await r.ListArtifactsAsync(testCancellationToken)).ToList().Take(n - n / denominator))
                {
                    await r.RemoveArtifactAsync(artifactInfo.Key, testCancellationToken);
                }
            }
            // get stable file size
            {
                var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
                using TestSqliteArtifactRegistrationManager r = new(tempFile, config);
                Assert.False(r.Context.UsingInMemory);
            }
            long fileSizeC = new FileInfo(tempFile).Length;
            // cleanup and measure
            {
                var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
                using TestSqliteArtifactRegistrationManager r = new(tempFile, config);
                Assert.False(r.Context.UsingInMemory);
                await r.CleanupDatabaseAsync(testCancellationToken);
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
