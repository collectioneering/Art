using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Art.Common.IO;
using Microsoft.Data.Sqlite;
using NUnit.Framework;

namespace Art.EF.Sqlite.Tests;

public class SqliteTests
{
    [Test]
    public async Task TestSqliteDatabaseFile()
    {
        string tempFile = ArtIOUtility.CreateRandomPath(Path.GetTempPath(), ".db");
        try
        {
            using SqliteArtifactRegistrationManager r = new(tempFile);
            await TestSqliteDatabase(r);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(tempFile);
        }
    }

    [Test]
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
                using SqliteArtifactRegistrationManager r = new(tempFile);
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
                using SqliteArtifactRegistrationManager r = new(tempFile);
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
                using SqliteArtifactRegistrationManager r = new(tempFile);
            }
            finally
            {
                SqliteConnection.ClearAllPools();
            }
            long fileSizeC = new FileInfo(tempFile).Length;
            // cleanup and measure
            try
            {
                using SqliteArtifactRegistrationManager r = new(tempFile);
                await r.CleanupDatabaseAsync();
            }
            finally
            {
                SqliteConnection.ClearAllPools();
            }
            long fileSizeD = new FileInfo(tempFile).Length;
            Assert.That(fileSizeD, Is.LessThan(fileSizeC));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(tempFile);
        }
    }

    [Test]
    public async Task TestSqliteDatabaseMemory()
    {
        using SqliteArtifactRegistrationManager r = new();
        await TestSqliteDatabase(r);
    }

    [Test]
    public async Task TestSqliteDatabaseMemoryExplicit()
    {
        using SqliteArtifactRegistrationManager r = new(true);
        await TestSqliteDatabase(r);
    }

    private static async Task TestSqliteDatabase(SqliteArtifactRegistrationManager r)
    {
        // 1
        ArtifactKey k1 = new("abec", "group1", "kraft1");
        ArtifactInfo i1 = new(k1, "name here", DateTimeOffset.FromUnixTimeSeconds(1636785227L));
        ArtifactResourceKey i1_k1 = new(k1, "file1");
        ArtifactResourceInfo i1_r1 = new(i1_k1, Updated: DateTimeOffset.Now, Version: "v0");
        ArtifactResourceKey i1_k2 = new(k1, "file2", "somepath");
        ArtifactResourceInfo i1_r2 = new(i1_k2);
        // 2
        ArtifactKey k2 = new("abec", "group1", "kraft2");
        ArtifactInfo i2 = new(k2, "some name here", DateTimeOffset.FromUnixTimeSeconds(1636785227L));
        ArtifactResourceKey i2_k1 = new(k2, "file1");
        ArtifactResourceInfo i2_r1 = new(i2_k1, Updated: DateTimeOffset.Now, Version: "v0");
        ArtifactResourceKey i2_k2 = new(k2, "file2", "somepath");
        ArtifactResourceInfo i2_r2 = new(i2_k2);
        ArtifactResourceKey i2_k3 = new(k2, "DUM");
        ArtifactResourceInfo i2_r3 = new(i2_k3);

        await r.AddArtifactAsync(i1);

        await r.AddResourceAsync(i1_r1);
        await r.AddResourceAsync(i1_r2);

        await r.AddArtifactAsync(i1); // dupe

        await r.AddArtifactAsync(i2);

        await r.AddResourceAsync(i2_r1);
        await r.AddResourceAsync(i2_r2);
        await r.AddResourceAsync(i2_r3);

        await r.RemoveArtifactAsync(k1);
        await r.RemoveResourceAsync(i2_k3);

        Assert.That(await r.TryGetArtifactAsync(k1), Is.Null);
        Assert.That(await r.TryGetResourceAsync(i1_k1), Is.Null);
        Assert.That(await r.TryGetResourceAsync(i1_k2), Is.Null);
        Assert.That(await r.TryGetArtifactAsync(k2), Is.EqualTo(i2));
        Assert.That(await r.TryGetResourceAsync(i2_k1), Is.EqualTo(i2_r1));
        Assert.That(await r.TryGetResourceAsync(i2_k2), Is.EqualTo(i2_r2));
        Assert.That(await r.TryGetResourceAsync(i2_k3), Is.Null);
        Assert.That((await r.ListArtifactsAsync("abec")).Count, Is.EqualTo(1));
        Assert.That((await r.ListArtifactsAsync("abec", "group1")).Count, Is.EqualTo(1));
        Assert.That((await r.ListArtifactsAsync("abec2", "group1")).Count, Is.EqualTo(0));
        Assert.That((await r.ListResourcesAsync(k2)).Count, Is.EqualTo(2));
        Assert.That((await r.ListResourcesAsync(k1)).Count, Is.EqualTo(0));
    }
}
