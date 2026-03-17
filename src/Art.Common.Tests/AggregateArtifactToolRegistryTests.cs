using System.Text.RegularExpressions;

namespace Art.Common.Tests;

public class AggregateArtifactToolRegistryTests
{
    private static readonly ArtifactToolID s_dummyToolId = new("Art.Common.Tests", "Art.Common.Tests.AggregateArtifactToolRegistryTestsDummyTool");

    private readonly AggregateArtifactToolRegistry _registry = new();
    private ArtifactToolRegistry? _subRegistry0;
    private ArtifactToolRegistry? _subRegistry1;

    [Fact]
    public void Add_Single_Succeeds()
    {
        _subRegistry0 = new ArtifactToolRegistry();
        _registry.Add(_subRegistry0);
    }

    [Fact]
    public void Add_Multiple_Succeeds()
    {
        _subRegistry0 = new ArtifactToolRegistry();
        _registry.Add(_subRegistry0);
        _subRegistry1 = new ArtifactToolRegistry();
        _registry.Add(_subRegistry1);
    }

    [Fact]
    public void Add_Duplicate_ThrowsArgumentException()
    {
        _subRegistry0 = new ArtifactToolRegistry();
        _registry.Add(_subRegistry0);
        Assert.Throws<ArgumentException>(() => _registry.Add(_subRegistry0));
    }

    [Fact]
    public void TryLoad_MultipleRegistries_LastHasApplicable_Success()
    {
        _subRegistry0 = new ArtifactToolRegistry();
        _subRegistry1 = new ArtifactToolRegistry();
        _subRegistry1.AddSelectable<AggregateArtifactToolRegistryTestsDummyTool>();
        _registry.Add(_subRegistry0);
        _registry.Add(_subRegistry1);
        bool success = _registry.TryLoad(s_dummyToolId, out IArtifactTool? tool);
        Assert.True(success);
        Assert.IsType<AggregateArtifactToolRegistryTestsDummyTool>(tool);
        tool.Dispose();
    }

    [Fact]
    public void TryLoad_MultipleRegistries_OtherThanLastHasApplicable_Success()
    {
        _subRegistry0 = new ArtifactToolRegistry();
        _subRegistry0.AddSelectable<AggregateArtifactToolRegistryTestsDummyTool>();
        _subRegistry1 = new ArtifactToolRegistry();
        _registry.Add(_subRegistry0);
        _registry.Add(_subRegistry1);
        bool success = _registry.TryLoad(s_dummyToolId, out IArtifactTool? tool);
        Assert.True(success);
        Assert.IsType<AggregateArtifactToolRegistryTestsDummyTool>(tool);
        tool.Dispose();
    }

    [Fact]
    public void TryLoad_DuplicatesInRegistries_LastUsedFirst()
    {
        ArtifactToolID sharedId = new("SharedAssembly", "SharedType");
        ArtifactToolID sharedId2 = new("SharedAssembly", "SharedType2");
        _subRegistry0 = new ArtifactToolRegistry();
        _subRegistry0.Add(new ArtifactToolSelectableRegistryEntry<AggregateArtifactToolRegistryTestsDummyTool>(sharedId));
        _subRegistry0.Add(new ArtifactToolSelectableRegistryEntry<AggregateArtifactToolRegistryTestsDummyTool2>(sharedId2));
        _subRegistry1 = new ArtifactToolRegistry();
        _subRegistry1.Add(new ArtifactToolSelectableRegistryEntry<AggregateArtifactToolRegistryTestsDummyTool2>(sharedId));
        _subRegistry1.Add(new ArtifactToolSelectableRegistryEntry<AggregateArtifactToolRegistryTestsDummyTool>(sharedId2));
        _registry.Add(_subRegistry0);
        _registry.Add(_subRegistry1);
        bool success = _registry.TryLoad(sharedId, out IArtifactTool? tool);
        Assert.True(success);
        Assert.IsType<AggregateArtifactToolRegistryTestsDummyTool2>(tool);
        tool.Dispose();
        bool success2 = _registry.TryLoad(sharedId2, out IArtifactTool? tool2);
        Assert.True(success2);
        Assert.IsType<AggregateArtifactToolRegistryTestsDummyTool>(tool2);
        tool2.Dispose();
    }

    [Fact]
    public void TryLoad_ValidToolId_Succeeds()
    {
        _subRegistry0 = new ArtifactToolRegistry();
        _registry.Add(_subRegistry0);
        _subRegistry0.AddSelectable<AggregateArtifactToolRegistryTestsDummyTool>();
        bool success = _registry.TryLoad(s_dummyToolId, out IArtifactTool? artifactTool);
        Assert.True(success);
        Assert.IsType<AggregateArtifactToolRegistryTestsDummyTool>(artifactTool);
        artifactTool.Dispose();
    }

    [Fact]
    public void TryLoad_InvalidToolId_Fails()
    {
        _subRegistry0 = new ArtifactToolRegistry();
        _registry.Add(_subRegistry0);
        _subRegistry0.AddSelectable<AggregateArtifactToolRegistryTestsDummyTool>();
        bool success = _registry.TryLoad(new ArtifactToolID("InvalidAssembly", "InvalidType"), out IArtifactTool? artifactTool);
        Assert.False(success);
        Assert.Null(artifactTool);
    }

    [Fact]
    public void TryIdentify_ValidKey_Succeeds()
    {
        _subRegistry0 = new ArtifactToolRegistry();
        _subRegistry0.AddSelectable<AggregateArtifactToolRegistryTestsDummyTool>();
        _registry.Add(_subRegistry0);
        bool success = _registry.TryIdentify("ID_1234", out ArtifactToolID? artifactToolId, out string? artifactId);
        Assert.True(success);
        Assert.Equal(s_dummyToolId, artifactToolId);
        Assert.Equal("1234", artifactId);
    }

    [Fact]
    public void TryIdentify_InvalidKey_Fails()
    {
        _subRegistry0 = new ArtifactToolRegistry();
        _registry.Add(_subRegistry0);
        _subRegistry0.AddSelectable<AggregateArtifactToolRegistryTestsDummyTool>();
        bool success = _registry.TryIdentify("INVALID_1234", out ArtifactToolID? artifactToolId, out string? artifactId);
        Assert.False(success);
        Assert.Null(artifactToolId);
        Assert.Null(artifactId);
    }

    [Fact]
    public void TryIdentifyAndLoad_ValidKey_Succeeds()
    {
        _subRegistry0 = new ArtifactToolRegistry();
        _registry.Add(_subRegistry0);
        _subRegistry0.AddSelectable<AggregateArtifactToolRegistryTestsDummyTool>();
        bool success = _registry.TryIdentify("ID_1234", out ArtifactToolID? artifactToolId, out string? artifactId);
        Assert.True(success);
        Assert.Equal(s_dummyToolId, artifactToolId);
        success = _registry.TryLoad(artifactToolId!, out IArtifactTool? artifactTool);
        Assert.True(success);
        Assert.IsType<AggregateArtifactToolRegistryTestsDummyTool>(artifactTool);
        Assert.Equal("1234", artifactId);
        artifactTool.Dispose();
    }

    [Fact]
    public void TryIdentifyAndLoad_InvalidKey_Fails()
    {
        _subRegistry0 = new ArtifactToolRegistry();
        _registry.Add(_subRegistry0);
        _subRegistry0.AddSelectable<AggregateArtifactToolRegistryTestsDummyTool>();
        bool success = _registry.TryIdentify("INVALID_1234", out ArtifactToolID? artifactToolId, out string? artifactId);
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
