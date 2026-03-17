using System.Text.RegularExpressions;

namespace Art.Common.Tests;

public class PluginTests
{
    private static readonly ArtifactToolID s_dummyToolId = new("Art.Common.Tests", "Art.Common.Tests.PluginTestTool");

    private readonly Plugin _registry = new Plugin(typeof(PluginTestTool).Assembly);

    [Fact]
    public void Generic_AllowsLoad()
    {
        bool success = _registry.TryLoad(s_dummyToolId, out IArtifactTool? artifactTool);
        Assert.True(success);
        Assert.IsType<PluginTestTool>(artifactTool);
        artifactTool.Dispose();
    }

    [Fact]
    public void Selectable_Generic_AllowsLoad()
    {
        bool success = _registry.TryLoad(s_dummyToolId, out IArtifactTool? artifactTool);
        Assert.True(success);
        Assert.IsType<PluginTestTool>(artifactTool);
        artifactTool.Dispose();
    }

    [Fact]
    public void Selectable_WithSelectableType_AllowsSelectionOfSelectable()
    {
        bool success = _registry.TryIdentify("PLUGINID_1234", out ArtifactToolID? artifactToolId, out string? artifactId);
        Assert.True(success);
        Assert.Equal(s_dummyToolId, artifactToolId);
        success = _registry.TryLoad(artifactToolId!, out IArtifactTool? artifactTool);
        Assert.True(success);
        Assert.IsType<PluginTestTool>(artifactTool);
        Assert.Equal("1234", artifactId);
        artifactTool.Dispose();
    }

    [Fact]
    public void TryLoad_ValidToolId_Succeeds()
    {
        bool success = _registry.TryLoad(s_dummyToolId, out IArtifactTool? artifactTool);
        Assert.True(success);
        Assert.IsType<PluginTestTool>(artifactTool);
        artifactTool.Dispose();
    }

    [Fact]
    public void TryLoad_InvalidToolId_Fails()
    {
        bool success = _registry.TryLoad(new ArtifactToolID("InvalidAssembly", "InvalidType"), out IArtifactTool? artifactTool);
        Assert.False(success);
        Assert.Null(artifactTool);
    }

    [Fact]
    public void TryIdentify_ValidKey_Succeeds()
    {
        bool success = _registry.TryIdentify("PLUGINID_1234", out ArtifactToolID? artifactToolId, out string? artifactId);
        Assert.True(success);
        Assert.Equal(s_dummyToolId, artifactToolId);
        Assert.Equal("1234", artifactId);
    }

    [Fact]
    public void TryIdentify_InvalidKey_Fails()
    {
        bool success = _registry.TryIdentify("INVALID_1234", out ArtifactToolID? artifactToolId, out string? artifactId);
        Assert.False(success);
        Assert.Null(artifactToolId);
        Assert.Null(artifactId);
    }

    [Fact]
    public void TryIdentifyAndLoad_ValidKey_Succeeds()
    {
        bool success = _registry.TryIdentify("PLUGINID_1234", out ArtifactToolID? artifactToolId, out string? artifactId);
        Assert.True(success);
        Assert.Equal(s_dummyToolId, artifactToolId);
        success = _registry.TryLoad(artifactToolId!, out IArtifactTool? artifactTool);
        Assert.True(success);
        Assert.IsType<PluginTestTool>(artifactTool);
        Assert.Equal("1234", artifactId);
        artifactTool.Dispose();
    }

    [Fact]
    public void TryIdentifyAndLoad_InvalidKey_Fails()
    {
        bool success = _registry.TryIdentify("INVALID_1234", out ArtifactToolID? artifactToolId, out string? artifactId);
        Assert.False(success);
        Assert.Null(artifactToolId);
        Assert.Null(artifactId);
    }
}

public partial class PluginTestTool : ArtifactTool, IArtifactToolSelfFactory<PluginTestTool>, IArtifactToolRegexSelector<PluginTestTool>
{
    [GeneratedRegex(@"^PLUGINID_(?<ID_GROUP>\d+)$")]
    public static partial Regex GetArtifactToolSelectorRegex();

    public static string GetArtifactToolSelectorRegexIdGroupName() => "ID_GROUP";
}
