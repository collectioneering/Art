using System.Text.Json;
using Art.Common.Proxies;
using Art.TestsBase;
using NUnit.Framework;

namespace Art.Common.Tests;

public class ArtifactToolListProxyTests
{
    [Test]
    public async Task FindOnlyTool_WithoutArtifactList_Throws()
    {
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
        await tool.InitializeAsync(profile: profile);
        var proxy = new ArtifactToolListProxy(tool, new ArtifactToolListOptions(), null);
        Assert.That(async () =>
        {
#if NET10_0_OR_GREATER
            return await AsyncEnumerable.ToListAsync(proxy.ListAsync());
#else
            return await proxy.ListAsync().ToListAsync();
#endif
        }, Throws.InstanceOf<NotSupportedException>());
    }

    [Test]
    public async Task FindOnlyTool_WithArtifactList_Success()
    {
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
        await tool.InitializeAsync(profile: profile);
        var proxy = new ArtifactToolListProxy(tool, new ArtifactToolListOptions(), null);
#if NET10_0_OR_GREATER
        var results = await AsyncEnumerable.ToListAsync(proxy.ListAsync());
#else
        var results = await proxy.ListAsync().ToListAsync();
#endif
        Assert.That(results.Select(v => int.Parse(v.Info.Key.Id)), Is.EquivalentTo([1, 2, 3]));
    }

    [Test]
    public async Task DumpOnlyTool_WithoutArtifactList_Success()
    {
        var profile = new ArtifactToolProfile("tool", null, null);
        var tool = new AsyncProgrammableArtifactDumpTool(async v =>
        {
            for (int i = 1; i <= 3; i++)
            {
                await v.DumpArtifactAsync(v.CreateData($"{i}"));
            }
        });
        await tool.InitializeAsync(profile: profile);
        var proxy = new ArtifactToolListProxy(tool, new ArtifactToolListOptions(), null);
#if NET10_0_OR_GREATER
        var results = await AsyncEnumerable.ToListAsync(proxy.ListAsync());
#else
        var results = await proxy.ListAsync().ToListAsync();
#endif
        Assert.That(results.Select(v => int.Parse(v.Info.Key.Id)), Is.EquivalentTo([1, 2, 3]));
    }

    [Test]
    public async Task DumpOnlyTool_WithArtifactList_DoesNotFilter()
    {
        var options = new Dictionary<string, JsonElement> { { "artifactList", JsonSerializer.SerializeToElement(new[] { "1", "2" }) } };
        var profile = new ArtifactToolProfile("tool", null, options);
        var tool = new AsyncProgrammableArtifactDumpTool(async v =>
        {
            for (int i = 1; i <= 3; i++)
            {
                await v.DumpArtifactAsync(v.CreateData($"{i}"));
            }
        });
        await tool.InitializeAsync(profile: profile);
        var proxy = new ArtifactToolListProxy(tool, new ArtifactToolListOptions(), null);
#if NET10_0_OR_GREATER
        var results = await AsyncEnumerable.ToListAsync(proxy.ListAsync());
#else
        var results = await proxy.ListAsync().ToListAsync();
#endif
        Assert.That(results.Select(v => int.Parse(v.Info.Key.Id)), Is.EquivalentTo([1, 2, 3]));
    }

    // TODO prioritisation tests
}
