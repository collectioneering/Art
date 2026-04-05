using System;
using System.Threading;
using System.Threading.Tasks;

namespace Art.DbTestsBase;

public abstract class ArtifactRegistrationManagerTestsBase
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

    protected static async Task TestReadWriteDatabase(IArtifactRegistrationManager r, CancellationToken testCancellationToken = default)
    {
        await r.AddArtifactAsync(i1, testCancellationToken);

        await r.AddResourceAsync(i1_r1, testCancellationToken);
        await r.AddResourceAsync(i1_r2, testCancellationToken);

        await r.AddArtifactAsync(i1, testCancellationToken); // dupe

        await r.AddArtifactAsync(i2, testCancellationToken);

        await r.AddResourceAsync(i2_r1, testCancellationToken);
        await r.AddResourceAsync(i2_r2, testCancellationToken);
        await r.AddResourceAsync(i2_r3, testCancellationToken);

        await r.RemoveArtifactAsync(k1, testCancellationToken);
        await r.RemoveResourceAsync(i2_k3, testCancellationToken);

        await VerifyWrittenDatabase(r, testCancellationToken);
    }

    protected static async Task VerifyWrittenDatabase(IArtifactRegistrationManager r, CancellationToken testCancellationToken = default)
    {
        Assert.Null(await r.TryGetArtifactAsync(k1, testCancellationToken));
        Assert.Null(await r.TryGetResourceAsync(i1_k1, testCancellationToken));
        Assert.Null(await r.TryGetResourceAsync(i1_k2, testCancellationToken));
        Assert.Equal(i2, await r.TryGetArtifactAsync(k2, testCancellationToken));
        Assert.Equal(i2_r1, await r.TryGetResourceAsync(i2_k1, testCancellationToken));
        Assert.Equal(i2_r2, await r.TryGetResourceAsync(i2_k2, testCancellationToken));
        Assert.Null(await r.TryGetResourceAsync(i2_k3, testCancellationToken));
        Assert.Single(await r.ListArtifactsAsync("abec", testCancellationToken));
        Assert.Single(await r.ListArtifactsAsync("abec", "group1", testCancellationToken));
        Assert.Empty(await r.ListArtifactsAsync("abec2", "group1", testCancellationToken));
        Assert.Equal(2, (await r.ListResourcesAsync(k2, testCancellationToken)).Count);
        Assert.Empty(await r.ListResourcesAsync(k1, testCancellationToken));
    }

    protected static async Task PrepareDatabaseForReadOnly(IArtifactRegistrationManager r, CancellationToken testCancellationToken = default)
    {
        await r.AddArtifactAsync(i1, testCancellationToken);

        await r.AddResourceAsync(i1_r1, testCancellationToken);
        await r.AddResourceAsync(i1_r2, testCancellationToken);

        await r.AddArtifactAsync(i1, testCancellationToken); // dupe

        await r.AddArtifactAsync(i2, testCancellationToken);

        await r.AddResourceAsync(i2_r1, testCancellationToken);
        await r.AddResourceAsync(i2_r2, testCancellationToken);
        await r.AddResourceAsync(i2_r3, testCancellationToken);

        await r.RemoveArtifactAsync(k1, testCancellationToken);
        await r.RemoveResourceAsync(i2_k3, testCancellationToken);

        await VerifyWrittenDatabase(r, testCancellationToken);
    }

    protected static async Task TestDatabaseReadOnly(IArtifactRegistrationManager r, bool testEmpty, CancellationToken testCancellationToken = default)
    {
        if (testEmpty)
        {
            Assert.Null(await r.TryGetArtifactAsync(k1, testCancellationToken));
            Assert.Null(await r.TryGetResourceAsync(i1_k1, testCancellationToken));
        }
        else
        {
            await VerifyWrittenDatabase(r, testCancellationToken);
        }

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await r.AddArtifactAsync(i1, testCancellationToken));
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await r.RemoveArtifactAsync(k2, testCancellationToken));

        if (testEmpty)
        {
            Assert.Null(await r.TryGetArtifactAsync(k1, testCancellationToken));
            Assert.Null(await r.TryGetResourceAsync(i1_k1, testCancellationToken));
        }
        else
        {
            await VerifyWrittenDatabase(r, testCancellationToken);
        }
    }
}
