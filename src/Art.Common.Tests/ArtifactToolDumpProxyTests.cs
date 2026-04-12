using System.Text;
using System.Text.Json;
using Art.Common.Management;
using Art.Common.Proxies;
using Art.TestsBase;
using Microsoft.Extensions.Time.Testing;

namespace Art.Common.Tests;

public class ArtifactToolDumpProxyTests
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
        var proxy = new ArtifactToolDumpProxy(
            tool,
            new ArtifactToolDumpOptions(),
            null);
        await Assert.ThrowsAsync<NotSupportedException>(async () => await proxy.DumpAsync(testCancellationToken));
    }

    [Fact]
    public async Task FindOnlyTool_WithArtifactList_Success()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        var options = new Dictionary<string, JsonElement> { { "artifactList", JsonSerializer.SerializeToElement(new[] { "1", "2", "3" }) } };
        var profile = new ArtifactToolProfile("tool", null, options);
        var arm = new InMemoryArtifactRegistrationManager();
        var config = new ArtifactToolConfig(arm, new NullArtifactDataManager(), NullExtensionsContext.Instance, new FakeTimeProvider(), true, true);
        var tool = new ProgrammableArtifactFindTool((v, k) =>
        {
            int i = int.Parse(k);
            if (1 <= i && i <= 3)
            {
                return v.CreateData($"{i}");
            }
            return null;
        });
        await tool.InitializeAsync(config: config, profile: profile, cancellationToken: testCancellationToken);
        var proxy = new ArtifactToolDumpProxy(
            tool,
            new ArtifactToolDumpOptions(),
            null);
        await proxy.DumpAsync(testCancellationToken);
        Assert.Equal(
            [1, 2, 3],
            (await arm.ListArtifactsAsync(testCancellationToken))
            .Select(v => int.Parse(v.Key.Id))
            .OrderBy(static v => v)
        );
    }

    private record CustomExportArtifactResourceInfo(
        Action CustomAction,
        string Resource,
        ArtifactResourceKey Key,
        string? ContentType = "text/plain",
        DateTimeOffset? Updated = null,
        DateTimeOffset? Retrieved = null,
        string? Version = null,
        Checksum? Checksum = null)
        : ArtifactResourceInfo(Key, ContentType, Updated, Retrieved, Version, Checksum)
    {
        /// <inheritdoc/>
        public override bool CanExportStream => true;

        /// <inheritdoc />
        public override bool CanGetStream => true;

        /// <inheritdoc/>
        public override async ValueTask ExportStreamAsync(Stream targetStream, ArtifactResourceExportOptions? exportOptions = null, CancellationToken cancellationToken = default)
        {
            CustomAction();
            await using StreamWriter sw = new(targetStream, Encoding.UTF8, leaveOpen: true);
            await sw.WriteAsync(Resource).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public override async ValueTask<Stream> GetStreamAsync(CancellationToken cancellationToken = default)
        {
            // streaming serialization is not supported, just https://www.youtube.com/watch?v=VQqO20pVhpk it
            var ms = new MemoryStream();
            await ExportStreamAsync(ms, cancellationToken: cancellationToken).ConfigureAwait(false);
            ms.Position = 0;
            return ms;
        }
    }

    [Fact]
    public async Task FindOnlyTool_RetrieveTimestamps_MatchesExpected()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        var options = new Dictionary<string, JsonElement> { { "artifactList", JsonSerializer.SerializeToElement(new[] { "1", "2", "3" }) } };
        var profile = new ArtifactToolProfile("tool", null, options);
        var arm = new InMemoryArtifactRegistrationManager();
        var timeProvider = new FakeTimeProvider(DateTimeOffset.FromUnixTimeMilliseconds(946713600000));
        var dict = new Dictionary<(int id, int? resourceIndex), DateTimeOffset>();
        var dict2 = new Dictionary<int, ArtifactKey>();
        var config = new ArtifactToolConfig(arm, new InMemoryArtifactDataManager(), NullExtensionsContext.Instance, timeProvider, true, true);
        var tool = new ProgrammableArtifactFindTool((v, k) =>
        {
            int i = int.Parse(k);
            if (1 <= i && i <= 3)
            {
                var data = v.CreateData($"{i}");
                dict[(i, null)] = timeProvider.GetUtcNow();
                timeProvider.Advance(TimeSpan.FromDays(1));
                dict2[i] = data.Info.Key;
                data.Add(new CustomExportArtifactResourceInfo(() =>
                    {
                        dict[(i, 0)] = timeProvider.GetUtcNow();
                        timeProvider.Advance(TimeSpan.FromDays(1));
                    }, $"{i}-0", new ArtifactResourceKey(data.Info.Key, "file")));
                return data;
            }
            return null;
        });
        await tool.InitializeAsync(config: config, profile: profile, testCancellationToken);
        var proxy = new ArtifactToolDumpProxy(
            tool,
            new ArtifactToolDumpOptions(),
            null);
        await proxy.DumpAsync(testCancellationToken);
        Assert.Equal(
            [
                (id: 1, timestamp: dict[(1, null)]), //
                (id: 2, timestamp: dict[(2, null)]), //
                (id: 3, timestamp: dict[(3, null)]) //
            ],
            (await arm.ListArtifactsAsync(testCancellationToken))
            .Select(v => (id: int.Parse(v.Key.Id), timestamp: v.RetrievalDate))
            .OrderBy(static v => v.id)
        );
        for (int i = 1; i <= 3; i++)
        {
            var res = (await arm.ListResourcesAsync(dict2[i], testCancellationToken))[0];
            Assert.Equal(dict[(i, 0)], res.Retrieved);
        }
    }

    [Fact]
    public async Task DumpOnlyTool_WithoutArtifactList_Success()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        var options = new Dictionary<string, JsonElement> { { "artifactList", JsonSerializer.SerializeToElement(new[] { "1", "2", "3" }) } };
        var profile = new ArtifactToolProfile("tool", null, options);
        var arm = new InMemoryArtifactRegistrationManager();
        var config = new ArtifactToolConfig(arm, new NullArtifactDataManager(), NullExtensionsContext.Instance, new FakeTimeProvider(), true, true);
        var tool = new AsyncProgrammableArtifactDumpTool(async v =>
        {
            for (int i = 1; i <= 3; i++)
            {
                await v.DumpArtifactAsync(v.CreateData($"{i}"), cancellationToken: testCancellationToken);
            }
        });
        await tool.InitializeAsync(config: config, profile: profile, cancellationToken: testCancellationToken);
        var proxy = new ArtifactToolDumpProxy(
            tool,
            new ArtifactToolDumpOptions(),
            null);
        await proxy.DumpAsync(testCancellationToken);
        Assert.Equal(
            [1, 2, 3],
            (await arm.ListArtifactsAsync(testCancellationToken))
            .Select(v => int.Parse(v.Key.Id))
            .OrderBy(static v => v)
        );
    }

    [Fact]
    public async Task DumpOnlyTool_WithArtifactList_DoesNotFilter()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        var options = new Dictionary<string, JsonElement> { { "artifactList", JsonSerializer.SerializeToElement(new[] { "1", "2" }) } };
        var profile = new ArtifactToolProfile("tool", null, options);
        var arm = new InMemoryArtifactRegistrationManager();
        var config = new ArtifactToolConfig(arm, new NullArtifactDataManager(), NullExtensionsContext.Instance, new FakeTimeProvider(), true, true);
        var tool = new AsyncProgrammableArtifactDumpTool(async v =>
        {
            for (int i = 1; i <= 3; i++)
            {
                await v.DumpArtifactAsync(v.CreateData($"{i}"));
            }
        });
        await tool.InitializeAsync(config: config, profile: profile, cancellationToken: testCancellationToken);
        var proxy = new ArtifactToolDumpProxy(
            tool,
            new ArtifactToolDumpOptions(),
            null);
        await proxy.DumpAsync(testCancellationToken);
        Assert.Equal(
            [1, 2, 3],
            (await arm.ListArtifactsAsync(testCancellationToken)).Select(v => int.Parse(v.Key.Id)).OrderBy(static v => v)
        );
    }

    // TODO prioritisation tests
}
