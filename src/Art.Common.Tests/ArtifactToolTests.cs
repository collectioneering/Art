using System.Text.Json;
using Art.Common.Management;
using Art.Common.Resources;
using Art.TestsBase;
using Microsoft.Extensions.Time.Testing;

namespace Art.Common.Tests;

public class ArtifactToolTests
{
    [Fact]
    public async Task ToolDisposed_DoesNotDisposeConfiguredManagers()
    {
        string toolString = ArtifactToolIDUtil.CreateToolString<ProgrammableArtifactFindTool>();
        var adm = new InMemoryArtifactDataManager();
        var arm = new InMemoryArtifactRegistrationManager();
        var fakeKey = new ArtifactResourceKey(new ArtifactKey("X", "Y", "Z"), "W");
        _ = await adm.ExistsAsync(fakeKey);
        _ = await arm.TryGetResourceAsync(fakeKey);
        var tool = new ArtifactTool();
        var profile = new ArtifactToolProfile(toolString, null, null);
        var config = new ArtifactToolConfig(arm, adm, new FakeTimeProvider(), true, true);
        await tool.InitializeAsync(config: config, profile: profile);
        Assert.Same(adm, tool.Config.DataManager);
        Assert.Same(arm, tool.Config.RegistrationManager);
        _ = await adm.ExistsAsync(fakeKey);
        _ = await arm.TryGetResourceAsync(fakeKey);
        tool.Dispose();
        _ = await adm.ExistsAsync(fakeKey);
        _ = await arm.TryGetResourceAsync(fakeKey);
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await tool.InitializeAsync());
        arm.Dispose();
        adm.Dispose();
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await adm.ExistsAsync(fakeKey));
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await arm.TryGetResourceAsync(fakeKey));
    }

    [Fact]
    public async Task ToolReinitialized_DoesNotDisposeConfiguredManagers()
    {
        string toolString = ArtifactToolIDUtil.CreateToolString<ProgrammableArtifactFindTool>();
        var adm = new InMemoryArtifactDataManager();
        var arm = new InMemoryArtifactRegistrationManager();
        var fakeKey = new ArtifactResourceKey(new ArtifactKey("X", "Y", "Z"), "W");
        _ = await adm.ExistsAsync(fakeKey);
        _ = await arm.TryGetResourceAsync(fakeKey);
        var tool = new ArtifactTool();
        var profile = new ArtifactToolProfile(toolString, null, null);
        var config = new ArtifactToolConfig(arm, adm, new FakeTimeProvider(), true, true);
        await tool.InitializeAsync(config: config, profile: profile);
        Assert.Same(adm, tool.Config.DataManager);
        Assert.Same(arm, tool.Config.RegistrationManager);
        _ = await adm.ExistsAsync(fakeKey);
        _ = await arm.TryGetResourceAsync(fakeKey);
        await tool.InitializeAsync(profile: profile);
        Assert.NotSame(adm, tool.Config.DataManager);
        Assert.NotSame(arm, tool.Config.RegistrationManager);
        _ = await adm.ExistsAsync(fakeKey);
        _ = await arm.TryGetResourceAsync(fakeKey);
        arm.Dispose();
        adm.Dispose();
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await adm.ExistsAsync(fakeKey));
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await arm.TryGetResourceAsync(fakeKey));
        await tool.InitializeAsync();
        tool.Dispose();
    }

    private class DummyGroupResolveArtifactTool : ArtifactTool
    {
        public override string GroupFallback { get; }

        public DummyGroupResolveArtifactTool(string groupFallback)
        {
            GroupFallback = groupFallback;
        }
    }

    [Fact]
    public async Task NoGroupInProfile_NoCustom_ResolvesFallback()
    {
        const string customFallback = "custom_fallback";
        using var tool = new DummyGroupResolveArtifactTool(customFallback);
        var profile = new ArtifactToolProfile("tool", null, null);
        await tool.InitializeAsync(profile: profile);
        Assert.Equal(customFallback, tool.ResolveGroup());
    }

    [Fact]
    public async Task NoGroupInProfile_Custom_ResolvesCustom()
    {
        const string customFallback = "custom_fallback";
        const string custom = "custom";
        using var tool = new DummyGroupResolveArtifactTool(customFallback);
        var profile = new ArtifactToolProfile("tool", null, null);
        await tool.InitializeAsync(profile: profile);
        Assert.Equal(custom, tool.ResolveGroup(custom));
    }

    [Fact]
    public async Task GroupInProfile_NoCustom_ResolvesProfile()
    {
        const string profileGroup = "profile_group";
        const string customFallback = "custom_fallback";
        using var tool = new DummyGroupResolveArtifactTool(customFallback);
        var profile = new ArtifactToolProfile("tool", profileGroup, null);
        await tool.InitializeAsync(profile: profile);
        Assert.Equal(profileGroup, tool.ResolveGroup());
    }

    [Fact]
    public async Task GroupInProfile_Custom_ResolvesProfile()
    {
        const string profileGroup = "profile_group";
        const string customFallback = "custom_fallback";
        const string custom = "custom";
        using var tool = new DummyGroupResolveArtifactTool(customFallback);
        var profile = new ArtifactToolProfile("tool", profileGroup, null);
        await tool.InitializeAsync(profile: profile);
        Assert.Equal(profileGroup, tool.ResolveGroup(custom));
    }

    [Fact]
    public async Task DefaultInit_UsesDefaults()
    {
        using var tool = new ProgrammableArtifactFindTool((_, _) => null);
        await tool.InitializeAsync();
        Assert.Equal(ArtifactToolIDUtil.CreateToolString<ProgrammableArtifactFindTool>(), tool.Profile.Tool);
        Assert.Null(tool.Profile.Group);
        Assert.Null(tool.Profile.Options);
        Assert.NotNull(tool.Config.DataManager);
        Assert.NotNull(tool.Config.RegistrationManager);
    }

    [Fact]
    public async Task SecondInit_ResetsDefaults()
    {
        const string toolString = "TOOL_1";
        const string group = "GROUP_1";
        using var tool = new ProgrammableArtifactFindTool((_, _) => null);
        var opts = new Dictionary<string, JsonElement> { { "OPT", JsonSerializer.SerializeToElement(1) } };
        var profile = new ArtifactToolProfile(toolString, group, opts);
        var adm = new InMemoryArtifactDataManager();
        var arm = new InMemoryArtifactRegistrationManager();
        var config = new ArtifactToolConfig(arm, adm, new FakeTimeProvider(), true, true);
        await tool.InitializeAsync(config: config, profile: profile);
        Assert.Equal(toolString, tool.Profile.Tool);
        Assert.Equal(group, tool.Profile.Group);
        Assert.NotNull(tool.Profile.Options);
        Assert.Single(tool.Profile.Options);
        Assert.Equal(1, tool.Profile.Options["OPT"].GetInt32());
        Assert.Same(adm, tool.Config.DataManager);
        Assert.Same(arm, tool.Config.RegistrationManager);
        await tool.InitializeAsync();
        Assert.Equal(ArtifactToolIDUtil.CreateToolString<ProgrammableArtifactFindTool>(), tool.Profile.Tool);
        Assert.Null(tool.Profile.Group);
        Assert.Null(tool.Profile.Options);
        Assert.NotNull(tool.Config.DataManager);
        Assert.NotNull(tool.Config.RegistrationManager);
    }

    [Fact]
    public async Task RepeatInit_ValuesUpdated()
    {
        using var tool = new ProgrammableArtifactFindTool((_, _) => null);
        for (int i = 0; i < 2; i++)
        {
            string toolString = $"TOOL_{i}";
            string group = $"GROUP_{i}";
            var opts = new Dictionary<string, JsonElement> { { "OPT", JsonSerializer.SerializeToElement(i) } };
            var profile = new ArtifactToolProfile(toolString, group, opts);
            var adm = new InMemoryArtifactDataManager();
            var arm = new InMemoryArtifactRegistrationManager();
            var config = new ArtifactToolConfig(arm, adm, new FakeTimeProvider(), true, true);
            await tool.InitializeAsync(config: config, profile: profile);
            Assert.Equal(toolString, tool.Profile.Tool);
            Assert.Equal(group, tool.Profile.Group);
            Assert.NotNull(tool.Profile.Options);
            Assert.Single(tool.Profile.Options);
            Assert.Equal(i, tool.Profile.Options["OPT"].GetInt32());
            Assert.Same(adm, tool.Config.DataManager);
            Assert.Same(arm, tool.Config.RegistrationManager);
        }
        await tool.InitializeAsync();
        Assert.Equal(ArtifactToolIDUtil.CreateToolString<ProgrammableArtifactFindTool>(), tool.Profile.Tool);
        Assert.Null(tool.Profile.Group);
        Assert.Null(tool.Profile.Options);
        Assert.NotNull(tool.Config.DataManager);
        Assert.NotNull(tool.Config.RegistrationManager);
    }

    [Fact]
    public async Task RepeatQueriesAfterOneInit_CompletesSuccessfully()
    {
        const string group = "GROUP_1";
        const string search = "ID_1";
        string toolString = ArtifactToolIDUtil.CreateToolString<ProgrammableArtifactFindTool>();
        using var tool = new ProgrammableArtifactFindTool((v, k) =>
        {
            if (search.Equals(k))
            {
                var data = v.CreateData(k);
                data.String("RES_1_CONTENT", "RES_1").Commit();
                return data;
            }
            return null;
        });
        var profile = new ArtifactToolProfile(toolString, group, null);
        await tool.InitializeAsync(profile: profile);
        for (int i = 0; i < 2; i++)
        {
            var data = await tool.FindAsync(search);
            Assert.NotNull(data);
            var key = data.Info.Key;
            Assert.Equal(search, key.Id);
            Assert.Equal(toolString, key.Tool);
            Assert.Equal(group, key.Group);
            var rkey1 = new ArtifactResourceKey(key, "RES_1");
            Assert.Equal([rkey1], data.Keys);
            var ari1 = data[rkey1];
            var stringArtifactResourceInfo = Assert.IsType<StringArtifactResourceInfo>(ari1);
            Assert.Equal("RES_1_CONTENT", stringArtifactResourceInfo.Resource);
        }
    }

    [Fact]
    public async Task RepeatInitPerQuery_CompletesSuccessfully()
    {
        const string group = "GROUP_1";
        const string search = "ID_1";
        string toolString = ArtifactToolIDUtil.CreateToolString<ProgrammableArtifactFindTool>();
        using var tool = new ProgrammableArtifactFindTool((v, k) =>
        {
            if (search.Equals(k))
            {
                var data = v.CreateData(k);
                data.String("RES_1_CONTENT", "RES_1").Commit();
                return data;
            }
            return null;
        });
        var profile = new ArtifactToolProfile(toolString, group, null);
        for (int i = 0; i < 2; i++)
        {
            await tool.InitializeAsync(profile: profile);
            var data = await tool.FindAsync(search);
            Assert.NotNull(data);
            var key = data.Info.Key;
            Assert.Equal(search, key.Id);
            Assert.Equal(toolString, key.Tool);
            Assert.Equal(group, key.Group);
            var rkey1 = new ArtifactResourceKey(key, "RES_1");
            Assert.Equal([rkey1], data.Keys);
            var ari1 = data[rkey1];
            var stringArtifactResourceInfo = Assert.IsType<StringArtifactResourceInfo>(ari1);
            Assert.Equal("RES_1_CONTENT", stringArtifactResourceInfo.Resource);
        }
    }
}
