using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Art.Common.Management;
using Art.Common.Resources;
using Art.TestsBase;
using Microsoft.Extensions.Time.Testing;
using NUnit.Framework;

namespace Art.Common.Tests;

public class ArtifactToolTests
{
    [Test]
    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    public async Task ToolDisposed_DoesNotDisposeConfiguredManagers()
    {
        string toolString = ArtifactToolIDUtil.CreateToolString<ProgrammableArtifactFindTool>();
        var adm = new InMemoryArtifactDataManager();
        var arm = new InMemoryArtifactRegistrationManager();
        var fakeKey = new ArtifactResourceKey(new ArtifactKey("X", "Y", "Z"), "W");
        Assert.That(async () => await adm.ExistsAsync(fakeKey), Throws.Nothing);
        Assert.That(async () => await arm.TryGetResourceAsync(fakeKey), Throws.Nothing);
        var tool = new ArtifactTool();
        var profile = new ArtifactToolProfile(toolString, null, null);
        var config = new ArtifactToolConfig(arm, adm, new FakeTimeProvider(), true, true);
        await tool.InitializeAsync(config: config, profile: profile);
        Assert.That(tool.Config.DataManager, Is.EqualTo(adm));
        Assert.That(tool.Config.RegistrationManager, Is.EqualTo(arm));
        Assert.That(async () => await adm.ExistsAsync(fakeKey), Throws.Nothing);
        Assert.That(async () => await arm.TryGetResourceAsync(fakeKey), Throws.Nothing);
        tool.Dispose();
        Assert.That(async () => await adm.ExistsAsync(fakeKey), Throws.Nothing);
        Assert.That(async () => await arm.TryGetResourceAsync(fakeKey), Throws.Nothing);
        Assert.That(async () => await tool.InitializeAsync(), Throws.InstanceOf<ObjectDisposedException>());
        arm.Dispose();
        adm.Dispose();
        Assert.That(async () => await adm.ExistsAsync(fakeKey), Throws.InstanceOf<ObjectDisposedException>());
        Assert.That(async () => await arm.TryGetResourceAsync(fakeKey), Throws.InstanceOf<ObjectDisposedException>());
    }

    [Test]
    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    public async Task ToolReinitialized_DoesNotDisposeConfiguredManagers()
    {
        string toolString = ArtifactToolIDUtil.CreateToolString<ProgrammableArtifactFindTool>();
        var adm = new InMemoryArtifactDataManager();
        var arm = new InMemoryArtifactRegistrationManager();
        var fakeKey = new ArtifactResourceKey(new ArtifactKey("X", "Y", "Z"), "W");
        Assert.That(async () => await adm.ExistsAsync(fakeKey), Throws.Nothing);
        Assert.That(async () => await arm.TryGetResourceAsync(fakeKey), Throws.Nothing);
        var tool = new ArtifactTool();
        var profile = new ArtifactToolProfile(toolString, null, null);
        var config = new ArtifactToolConfig(arm, adm, new FakeTimeProvider(), true, true);
        await tool.InitializeAsync(config: config, profile: profile);
        Assert.That(tool.Config.DataManager, Is.EqualTo(adm));
        Assert.That(tool.Config.RegistrationManager, Is.EqualTo(arm));
        Assert.That(async () => await adm.ExistsAsync(fakeKey), Throws.Nothing);
        Assert.That(async () => await arm.TryGetResourceAsync(fakeKey), Throws.Nothing);
        await tool.InitializeAsync(profile: profile);
        Assert.That(tool.Config.DataManager, Is.Not.EqualTo(adm));
        Assert.That(tool.Config.RegistrationManager, Is.Not.EqualTo(arm));
        Assert.That(async () => await adm.ExistsAsync(fakeKey), Throws.Nothing);
        Assert.That(async () => await arm.TryGetResourceAsync(fakeKey), Throws.Nothing);
        arm.Dispose();
        adm.Dispose();
        Assert.That(async () => await adm.ExistsAsync(fakeKey), Throws.InstanceOf<ObjectDisposedException>());
        Assert.That(async () => await arm.TryGetResourceAsync(fakeKey), Throws.InstanceOf<ObjectDisposedException>());
        Assert.That(async () => await tool.InitializeAsync(), Throws.Nothing);
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

    [Test]
    public async Task NoGroupInProfile_NoCustom_ResolvesFallback()
    {
        const string customFallback = "custom_fallback";
        using var tool = new DummyGroupResolveArtifactTool(customFallback);
        var profile = new ArtifactToolProfile("tool", null, null);
        await tool.InitializeAsync(profile: profile);
        Assert.That(tool.ResolveGroup(), Is.EqualTo(customFallback));
    }

    [Test]
    public async Task NoGroupInProfile_Custom_ResolvesCustom()
    {
        const string customFallback = "custom_fallback";
        const string custom = "custom";
        using var tool = new DummyGroupResolveArtifactTool(customFallback);
        var profile = new ArtifactToolProfile("tool", null, null);
        await tool.InitializeAsync(profile: profile);
        Assert.That(tool.ResolveGroup(custom), Is.EqualTo(custom));
    }

    [Test]
    public async Task GroupInProfile_NoCustom_ResolvesProfile()
    {
        const string profileGroup = "profile_group";
        const string customFallback = "custom_fallback";
        using var tool = new DummyGroupResolveArtifactTool(customFallback);
        var profile = new ArtifactToolProfile("tool", profileGroup, null);
        await tool.InitializeAsync(profile: profile);
        Assert.That(tool.ResolveGroup(), Is.EqualTo(profileGroup));
    }

    [Test]
    public async Task GroupInProfile_Custom_ResolvesProfile()
    {
        const string profileGroup = "profile_group";
        const string customFallback = "custom_fallback";
        const string custom = "custom";
        using var tool = new DummyGroupResolveArtifactTool(customFallback);
        var profile = new ArtifactToolProfile("tool", profileGroup, null);
        await tool.InitializeAsync(profile: profile);
        Assert.That(tool.ResolveGroup(custom), Is.EqualTo(profileGroup));
    }

    [Test]
    public async Task DefaultInit_UsesDefaults()
    {
        using var tool = new ProgrammableArtifactFindTool((_, _) => null);
        await tool.InitializeAsync();
        Assert.That(tool.Profile.Tool, Is.EqualTo(ArtifactToolIDUtil.CreateToolString<ProgrammableArtifactFindTool>()));
        Assert.That(tool.Profile.Group, Is.Null);
        Assert.That(tool.Profile.Options, Is.Null);
        Assert.That(tool.Config.DataManager, Is.Not.Null);
        Assert.That(tool.Config.RegistrationManager, Is.Not.Null);
    }

    [Test]
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
        Assert.That(tool.Profile.Tool, Is.EqualTo(toolString));
        Assert.That(tool.Profile.Group, Is.EqualTo(group));
        Assert.That(tool.Profile.Options, Has.Count.EqualTo(1));
        Assert.That(tool.Profile.Options!["OPT"].GetInt32(), Is.EqualTo(1));
        Assert.That(tool.Config.DataManager, Is.EqualTo(adm));
        Assert.That(tool.Config.RegistrationManager, Is.EqualTo(arm));
        await tool.InitializeAsync();
        Assert.That(tool.Profile.Tool, Is.EqualTo(ArtifactToolIDUtil.CreateToolString<ProgrammableArtifactFindTool>()));
        Assert.That(tool.Profile.Group, Is.Null);
        Assert.That(tool.Profile.Options, Is.Null);
        Assert.That(tool.Config.DataManager, Is.Not.Null);
        Assert.That(tool.Config.RegistrationManager, Is.Not.Null);
    }

    [Test]
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
            Assert.That(tool.Profile.Tool, Is.EqualTo(toolString));
            Assert.That(tool.Profile.Group, Is.EqualTo(group));
            Assert.That(tool.Profile.Options, Has.Count.EqualTo(1));
            Assert.That(tool.Profile.Options!["OPT"].GetInt32(), Is.EqualTo(i));
            Assert.That(tool.Config.DataManager, Is.EqualTo(adm));
            Assert.That(tool.Config.RegistrationManager, Is.EqualTo(arm));
        }
        await tool.InitializeAsync();
        Assert.That(tool.Profile.Tool, Is.EqualTo(ArtifactToolIDUtil.CreateToolString<ProgrammableArtifactFindTool>()));
        Assert.That(tool.Profile.Group, Is.Null);
        Assert.That(tool.Profile.Options, Is.Null);
        Assert.That(tool.Config.DataManager, Is.Not.Null);
        Assert.That(tool.Config.RegistrationManager, Is.Not.Null);
    }

    [Test]
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
            Assert.That(data, Is.Not.Null);
            var key = data!.Info.Key;
            Assert.That(key.Id, Is.EqualTo(search));
            Assert.That(key.Tool, Is.EqualTo(toolString));
            Assert.That(key.Group, Is.EqualTo(group));
            var rkey1 = new ArtifactResourceKey(key, "RES_1");
            Assert.That(data.Keys, Is.EquivalentTo([rkey1]));
            Assert.That(data[rkey1], Is.InstanceOf<StringArtifactResourceInfo>().With.Property("Resource").EqualTo("RES_1_CONTENT"));
        }
    }

    [Test]
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
            Assert.That(data, Is.Not.Null);
            var key = data!.Info.Key;
            Assert.That(key.Id, Is.EqualTo(search));
            Assert.That(key.Tool, Is.EqualTo(toolString));
            Assert.That(key.Group, Is.EqualTo(group));
            var rkey1 = new ArtifactResourceKey(key, "RES_1");
            Assert.That(data.Keys, Is.EquivalentTo([rkey1]));
            Assert.That(data[rkey1], Is.InstanceOf<StringArtifactResourceInfo>().With.Property("Resource").EqualTo("RES_1_CONTENT"));
        }
    }
}
