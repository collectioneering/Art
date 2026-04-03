using System.Text.Json;
using Art.Common.Proxies;
using Art.TestsBase;

namespace Art.Common.Tests;

public class ArtifactToolListProxyTests
{
    [Fact]
    public async Task FindOnlyTool_WithoutArtifactList_Throws()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        var profile = new ArtifactToolProfile("tool", null, null);
        var tool = new ProgrammableArtifactFindTool((v, k) =>
        {
            int i = int.Parse(k);
            if (1 <= i && i <= 3)
            {
                return v.CreateData($"{i}");
            }

            return null;
        });
        await tool.InitializeAsync(profile: profile, cancellationToken: testCancellationToken);
        var proxy = new ArtifactToolListProxy(
            tool,
            new ArtifactToolListOptions(),
            null);
        await Assert.ThrowsAsync<NotSupportedException>(async () =>
        {
#if NET10_0_OR_GREATER
            _ = await proxy.ListAsync(cancellationToken: testCancellationToken).ToListAsync(testCancellationToken);
#else
            _ = await proxy.ListAsync(cancellationToken: testCancellationToken).ConvertToListAsync(testCancellationToken);
#endif
        });
    }

    [Fact]
    public async Task FindOnlyTool_WithArtifactList_Success()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        var options = new Dictionary<string, JsonElement> { { "artifactList", JsonSerializer.SerializeToElement(new[] { "1", "2", "3" }) } };
        var profile = new ArtifactToolProfile("tool", null, options);
        var tool = new ProgrammableArtifactFindTool((v, k) =>
        {
            int i = int.Parse(k);
            if (1 <= i && i <= 3)
            {
                return v.CreateData($"{i}");
            }

            return null;
        });
        await tool.InitializeAsync(profile: profile, cancellationToken: testCancellationToken);
        var proxy = new ArtifactToolListProxy(
            tool,
            new ArtifactToolListOptions(),
            null);
#if NET10_0_OR_GREATER
        var results = await proxy.ListAsync(cancellationToken: testCancellationToken).ToListAsync(testCancellationToken);
#else
        var results = await proxy.ListAsync(cancellationToken: testCancellationToken).ConvertToListAsync(testCancellationToken);
#endif
        Assert.Equal(
            [1, 2, 3],
            results.Select(v => int.Parse(v.Info.Key.Id)).OrderBy(static v => v)
        );
    }

    [Fact]
    public async Task DumpOnlyTool_WithoutArtifactList_Success()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        var profile = new ArtifactToolProfile("tool", null, null);
        var tool = new AsyncProgrammableArtifactDumpTool(async v =>
        {
            for (int i = 1; i <= 3; i++)
            {
                await v.DumpArtifactAsync(v.CreateData($"{i}"), cancellationToken: testCancellationToken);
            }
        });
        await tool.InitializeAsync(profile: profile, cancellationToken: testCancellationToken);
        var proxy = new ArtifactToolListProxy(
            tool,
            new ArtifactToolListOptions(),
            null);
#if NET10_0_OR_GREATER
        var results = await proxy.ListAsync(cancellationToken: testCancellationToken).ToListAsync(testCancellationToken);
#else
        var results = await proxy.ListAsync(cancellationToken: testCancellationToken).ConvertToListAsync(testCancellationToken);
#endif
        Assert.Equal(
            [1, 2, 3],
            results.Select(v => int.Parse(v.Info.Key.Id)).OrderBy(static v => v)
        );
    }

    [Fact]
    public async Task DumpOnlyTool_WithArtifactList_DoesNotFilter()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        var options = new Dictionary<string, JsonElement> { { "artifactList", JsonSerializer.SerializeToElement(new[] { "1", "2" }) } };
        var profile = new ArtifactToolProfile("tool", null, options);
        var tool = new AsyncProgrammableArtifactDumpTool(async v =>
        {
            for (int i = 1; i <= 3; i++)
            {
                await v.DumpArtifactAsync(v.CreateData($"{i}"));
            }
        });
        await tool.InitializeAsync(profile: profile, cancellationToken: testCancellationToken);
        var proxy = new ArtifactToolListProxy(
            tool,
            new ArtifactToolListOptions(),
            null);
#if NET10_0_OR_GREATER
        var results = await proxy.ListAsync(cancellationToken: testCancellationToken).ToListAsync(testCancellationToken);
#else
        var results = await proxy.ListAsync(cancellationToken: testCancellationToken).ConvertToListAsync(testCancellationToken);
#endif
        Assert.Equal(
            [1, 2, 3],
            results.Select(v => int.Parse(v.Info.Key.Id)).OrderBy(static v => v)
        );
    }

    // TODO prioritisation tests
}
