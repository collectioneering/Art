using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Art.Common.IO;
using Art.EF.Sqlite.Tests.Migrations1;
using Art.EF.Sqlite.Tests.Migrations2;
using Microsoft.Data.Sqlite;

namespace Art.EF.Sqlite.Tests;

public class SqliteTestsBase
{
    // 1
    private static ArtifactKey k1 = new("abec", "group1", "kraft1");
    private static ArtifactInfo i1 = new(k1, "name here", DateTimeOffset.FromUnixTimeSeconds(1636785227L));
    private static ArtifactResourceKey i1_k1 = new(k1, "file1");
    private static ArtifactResourceInfo i1_r1 = new(i1_k1, Updated: DateTimeOffset.Now, Version: "v0");
    private static ArtifactResourceKey i1_k2 = new(k1, "file2", "somepath");
    private static ArtifactResourceInfo i1_r2 = new(i1_k2);
    // 2
    private static ArtifactKey k2 = new("abec", "group1", "kraft2");
    private static ArtifactInfo i2 = new(k2, "some name here", DateTimeOffset.FromUnixTimeSeconds(1636785227L));
    private static ArtifactResourceKey i2_k1 = new(k2, "file1");
    private static ArtifactResourceInfo i2_r1 = new(i2_k1, Updated: DateTimeOffset.Now, Version: "v0");
    private static ArtifactResourceKey i2_k2 = new(k2, "file2", "somepath");
    private static ArtifactResourceInfo i2_r2 = new(i2_k2);
    private static ArtifactResourceKey i2_k3 = new(k2, "DUM");
    private static ArtifactResourceInfo i2_r3 = new(i2_k3);

    protected static async Task TestSqliteDatabase(SqliteArtifactRegistrationManager r)
    {
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

        await VerifyWrittenSqliteDatabase(r);
    }

    protected static async Task VerifyWrittenSqliteDatabase(SqliteArtifactRegistrationManager r)
    {
        Assert.Null(await r.TryGetArtifactAsync(k1));
        Assert.Null(await r.TryGetResourceAsync(i1_k1));
        Assert.Null(await r.TryGetResourceAsync(i1_k2));
        Assert.Equal(i2, await r.TryGetArtifactAsync(k2));
        Assert.Equal(i2_r1, await r.TryGetResourceAsync(i2_k1));
        Assert.Equal(i2_r2, await r.TryGetResourceAsync(i2_k2));
        Assert.Null(await r.TryGetResourceAsync(i2_k3));
        Assert.Single(await r.ListArtifactsAsync("abec"));
        Assert.Single(await r.ListArtifactsAsync("abec", "group1"));
        Assert.Empty(await r.ListArtifactsAsync("abec2", "group1"));
        Assert.Equal(2, (await r.ListResourcesAsync(k2)).Count);
        Assert.Empty(await r.ListResourcesAsync(k1));
    }

    protected static async Task PrepareSqliteDatabaseForReadOnly(SqliteArtifactRegistrationManager r)
    {
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

        await VerifyWrittenSqliteDatabase(r);
    }

    protected static async Task TestSqliteDatabaseReadOnly(SqliteArtifactRegistrationManager r, bool testEmpty)
    {
        if (!testEmpty)
        {
            await VerifyWrittenSqliteDatabase(r);
        }

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await r.AddArtifactAsync(i1));
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await r.RemoveArtifactAsync(k2));

        if (!testEmpty)
        {
            await VerifyWrittenSqliteDatabase(r);
        }
    }
}
