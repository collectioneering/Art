using System.Text.RegularExpressions;

namespace Art.Common.Tests;

public class AggregateArtifactToolRegistryTests
{
    private static readonly ArtifactToolID s_dummyToolId = new("Art.Common.Tests", "Art.Common.Tests.AggregateArtifactToolRegistryTestsDummyTool");

    [Fact]
    public void TryLoad_MultipleRegistries_FirstHasApplicable_Success()
    {
        var subRegistry0 = new ArtifactToolRegistry();
        subRegistry0.AddSelectable<AggregateArtifactToolRegistryTestsDummyTool>();
        var subRegistry1 = new ArtifactToolRegistry();
        var registry = new AggregateArtifactToolRegistry([subRegistry0, subRegistry1]);
        bool success = registry.TryLoad(s_dummyToolId, out IArtifactTool? tool);
        Assert.True(success);
        Assert.IsType<AggregateArtifactToolRegistryTestsDummyTool>(tool);
        tool.Dispose();
    }

    [Fact]
    public void TryLoad_MultipleRegistries_OtherThanFirstHasApplicable_Success()
    {
        var subRegistry0 = new ArtifactToolRegistry();
        var subRegistry1 = new ArtifactToolRegistry();
        subRegistry1.AddSelectable<AggregateArtifactToolRegistryTestsDummyTool>();
        var registry = new AggregateArtifactToolRegistry([subRegistry0, subRegistry1]);
        bool success = registry.TryLoad(s_dummyToolId, out IArtifactTool? tool);
        Assert.True(success);
        Assert.IsType<AggregateArtifactToolRegistryTestsDummyTool>(tool);
        tool.Dispose();
    }

    [Fact]
    public void TryLoad_DuplicatesInRegistries_FirstUsedFirst()
    {
        ArtifactToolID sharedId = new("SharedAssembly", "SharedType");
        ArtifactToolID sharedId2 = new("SharedAssembly", "SharedType2");
        var subRegistry0 = new ArtifactToolRegistry();
        subRegistry0.Add(new ArtifactToolSelectableRegistryEntry<AggregateArtifactToolRegistryTestsDummyTool>(sharedId));
        subRegistry0.Add(new ArtifactToolSelectableRegistryEntry<AggregateArtifactToolRegistryTestsDummyTool2>(sharedId2));
        var subRegistry1 = new ArtifactToolRegistry();
        subRegistry1.Add(new ArtifactToolSelectableRegistryEntry<AggregateArtifactToolRegistryTestsDummyTool3>(sharedId));
        subRegistry1.Add(new ArtifactToolSelectableRegistryEntry<AggregateArtifactToolRegistryTestsDummyTool4>(sharedId2));
        var registry = new AggregateArtifactToolRegistry([subRegistry0, subRegistry1]);
        bool success = registry.TryLoad(sharedId, out IArtifactTool? tool);
        Assert.True(success);
        Assert.IsType<AggregateArtifactToolRegistryTestsDummyTool>(tool);
        tool.Dispose();
        bool success2 = registry.TryLoad(sharedId2, out IArtifactTool? tool2);
        Assert.True(success2);
        Assert.IsType<AggregateArtifactToolRegistryTestsDummyTool2>(tool2);
        tool2.Dispose();
    }

    [Fact]
    public void TryLoad_ValidToolId_Succeeds()
    {
        var subRegistry0 = new ArtifactToolRegistry();
        var registry = new AggregateArtifactToolRegistry([subRegistry0]);
        subRegistry0.AddSelectable<AggregateArtifactToolRegistryTestsDummyTool>();
        bool success = registry.TryLoad(s_dummyToolId, out IArtifactTool? artifactTool);
        Assert.True(success);
        Assert.IsType<AggregateArtifactToolRegistryTestsDummyTool>(artifactTool);
        artifactTool.Dispose();
    }

    [Fact]
    public void TryLoad_InvalidToolId_Fails()
    {
        var subRegistry0 = new ArtifactToolRegistry();
        var registry = new AggregateArtifactToolRegistry([subRegistry0]);
        subRegistry0.AddSelectable<AggregateArtifactToolRegistryTestsDummyTool>();
        bool success = registry.TryLoad(new ArtifactToolID("InvalidAssembly", "InvalidType"), out IArtifactTool? artifactTool);
        Assert.False(success);
        Assert.Null(artifactTool);
    }

    [Fact]
    public void TryIdentify_ValidKey_Succeeds()
    {
        var subRegistry0 = new ArtifactToolRegistry();
        subRegistry0.AddSelectable<AggregateArtifactToolRegistryTestsDummyTool>();
        var registry = new AggregateArtifactToolRegistry([subRegistry0]);
        bool success = registry.TryIdentify("ID_1234", out ArtifactToolID? artifactToolId, out string? artifactId);
        Assert.True(success);
        Assert.Equal(s_dummyToolId, artifactToolId);
        Assert.Equal("1234", artifactId);
    }

    [Fact]
    public void TryIdentify_InvalidKey_Fails()
    {
        var subRegistry0 = new ArtifactToolRegistry();
        var registry = new AggregateArtifactToolRegistry([subRegistry0]);
        subRegistry0.AddSelectable<AggregateArtifactToolRegistryTestsDummyTool>();
        bool success = registry.TryIdentify("INVALID_1234", out ArtifactToolID? artifactToolId, out string? artifactId);
        Assert.False(success);
        Assert.Null(artifactToolId);
        Assert.Null(artifactId);
    }

    [Fact]
    public void TryIdentifyAndLoad_ValidKey_Succeeds()
    {
        var subRegistry0 = new ArtifactToolRegistry();
        var registry = new AggregateArtifactToolRegistry([subRegistry0]);
        subRegistry0.AddSelectable<AggregateArtifactToolRegistryTestsDummyTool>();
        bool success = registry.TryIdentify("ID_1234", out ArtifactToolID? artifactToolId, out string? artifactId);
        Assert.True(success);
        Assert.Equal(s_dummyToolId, artifactToolId);
        success = registry.TryLoad(artifactToolId!, out IArtifactTool? artifactTool);
        Assert.True(success);
        Assert.IsType<AggregateArtifactToolRegistryTestsDummyTool>(artifactTool);
        Assert.Equal("1234", artifactId);
        artifactTool.Dispose();
    }

    [Fact]
    public void TryIdentifyAndLoad_InvalidKey_Fails()
    {
        var subRegistry0 = new ArtifactToolRegistry();
        var registry = new AggregateArtifactToolRegistry([subRegistry0]);
        subRegistry0.AddSelectable<AggregateArtifactToolRegistryTestsDummyTool>();
        bool success = registry.TryIdentify("INVALID_1234", out ArtifactToolID? artifactToolId, out string? artifactId);
        Assert.False(success);
        Assert.Null(artifactToolId);
        Assert.Null(artifactId);
    }
}

internal partial class AggregateArtifactToolRegistryTestsDummyTool : ArtifactTool, IArtifactToolSelfFactory<AggregateArtifactToolRegistryTestsDummyTool>, IArtifactToolRegexSelector<AggregateArtifactToolRegistryTestsDummyTool>
{
    [GeneratedRegex(@"^ID_(?<ID_GROUP>\d+)$")]
    public static partial Regex GetArtifactToolSelectorRegex();

    public static string GetArtifactToolSelectorRegexIdGroupName() => "ID_GROUP";
}

internal partial class AggregateArtifactToolRegistryTestsDummyTool2 : ArtifactTool, IArtifactToolSelfFactory<AggregateArtifactToolRegistryTestsDummyTool2>, IArtifactToolRegexSelector<AggregateArtifactToolRegistryTestsDummyTool2>
{
    [GeneratedRegex(@"^ID2_(?<ID_GROUP>\d+)$")]
    public static partial Regex GetArtifactToolSelectorRegex();

    public static string GetArtifactToolSelectorRegexIdGroupName() => "ID_GROUP";
}

internal partial class AggregateArtifactToolRegistryTestsDummyTool3 : ArtifactTool, IArtifactToolSelfFactory<AggregateArtifactToolRegistryTestsDummyTool3>, IArtifactToolRegexSelector<AggregateArtifactToolRegistryTestsDummyTool3>
{
    [GeneratedRegex(@"^ID3_(?<ID_GROUP>\d+)$")]
    public static partial Regex GetArtifactToolSelectorRegex();

    public static string GetArtifactToolSelectorRegexIdGroupName() => "ID_GROUP";
}


internal partial class AggregateArtifactToolRegistryTestsDummyTool4 : ArtifactTool, IArtifactToolSelfFactory<AggregateArtifactToolRegistryTestsDummyTool3>, IArtifactToolRegexSelector<AggregateArtifactToolRegistryTestsDummyTool3>
{
    [GeneratedRegex(@"^ID4_(?<ID_GROUP>\d+)$")]
    public static partial Regex GetArtifactToolSelectorRegex();

    public static string GetArtifactToolSelectorRegexIdGroupName() => "ID_GROUP";
}
