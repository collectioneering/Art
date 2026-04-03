namespace Art.Common.Tests;

public class ArtifactToolArtifactDataTests
{
    private const string ProfileGroup = "ProfileGroup";
    private const string CustomGroup = "CustomGroup";
    private const string FallbackGroup = "FallbackGroup";

    private readonly CustomGroupArtifactTool _tool = new();

    [Fact]
    public async Task CreateData_CustomGroup_NoProfileGroup_CustomGroupApplied()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        await _tool.InitializeAsync(profile: CreateProfile(null), cancellationToken: testCancellationToken);
        var data = _tool.CreateData("id", group: CustomGroup);
        Assert.Equal(CustomGroup, data.Info.Key.Group);
        Assert.NotEqual(ProfileGroup, data.Info.Key.Group);
        Assert.NotEqual(FallbackGroup, data.Info.Key.Group);
    }

    [Fact]
    public async Task CreateData_CustomGroup_ProfileGroup_ProfileGroupApplied()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        await _tool.InitializeAsync(profile: CreateProfile(ProfileGroup), cancellationToken: testCancellationToken);
        var data = _tool.CreateData("id", group: CustomGroup);
        Assert.Equal(ProfileGroup, data.Info.Key.Group);
        Assert.NotEqual(CustomGroup, data.Info.Key.Group);
        Assert.NotEqual(FallbackGroup, data.Info.Key.Group);
    }

    [Fact]
    public async Task CreateData_NoCustomGroup_ProfileGroup_ProfileGroupApplied()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        await _tool.InitializeAsync(profile: CreateProfile(ProfileGroup), cancellationToken: testCancellationToken);
        var data = _tool.CreateData("id");
        Assert.Equal(ProfileGroup, data.Info.Key.Group);
        Assert.NotEqual(CustomGroup, data.Info.Key.Group);
        Assert.NotEqual(FallbackGroup, data.Info.Key.Group);
    }

    [Fact]
    public async Task CreateData_NoCustomGroup_NoProfileGroup_FallbackGroupApplied()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        await _tool.InitializeAsync(profile: CreateProfile(null), cancellationToken: testCancellationToken);
        var data = _tool.CreateData("id");
        Assert.Equal(FallbackGroup, data.Info.Key.Group);
        Assert.NotEqual(CustomGroup, data.Info.Key.Group);
        Assert.NotEqual(ProfileGroup, data.Info.Key.Group);
    }

    private ArtifactToolProfile CreateProfile(string? group)
    {
        return new ArtifactToolProfile(ArtifactToolIDUtil.CreateToolString(_tool.GetType()), group, null);
    }

    private class CustomGroupArtifactTool : ArtifactTool
    {
        public override string GroupFallback => FallbackGroup;
    }
}
