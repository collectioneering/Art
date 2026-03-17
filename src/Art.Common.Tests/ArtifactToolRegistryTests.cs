using System.Text.RegularExpressions;

namespace Art.Common.Tests;

public class ArtifactToolRegistryTests
{
    private static readonly ArtifactToolID s_dummyToolId = new("Art.Common.Tests", "Art.Common.Tests.ArtifactToolRegistryTestsDummyTool");
    private static readonly ArtifactToolID s_customToolId = new("CustomAssembly", "CustomType");

    private readonly ArtifactToolRegistry _registry = new();

    [Fact]
    public void Add_Generic_AllowsLoad()
    {
        _registry.AddSelectable<ArtifactToolRegistryTestsDummyTool>();
        bool success = _registry.TryLoad(s_dummyToolId, out IArtifactTool? artifactTool);
        Assert.True(success);
        Assert.IsType<ArtifactToolRegistryTestsDummyTool>(artifactTool);
        artifactTool.Dispose();
    }

    [Fact]
    public void AddSelectable_Generic_AllowsLoad()
    {
        _registry.AddSelectable<ArtifactToolRegistryTestsDummyTool>();
        bool success = _registry.TryLoad(s_dummyToolId, out IArtifactTool? artifactTool);
        Assert.True(success);
        Assert.IsType<ArtifactToolRegistryTestsDummyTool>(artifactTool);
        artifactTool.Dispose();
    }

    [Fact]
    public void Add_DuplicateButCustomId_AllowsLoad()
    {
        _registry.AddSelectable<ArtifactToolRegistryTestsDummyTool>();
        _registry.Add(new ArtifactToolSelectableRegistryEntry<ArtifactToolRegistryTestsDummyTool>(s_customToolId));
        bool success = _registry.TryLoad(s_dummyToolId, out IArtifactTool? artifactTool);
        Assert.True(success);
        Assert.IsType<ArtifactToolRegistryTestsDummyTool>(artifactTool);
        artifactTool.Dispose();
    }

    [Fact]
    public void Add_DuplicateGeneric_ThrowsArgumentException()
    {
        _registry.AddSelectable<ArtifactToolRegistryTestsDummyTool>();
        Assert.Throws<ArgumentException>(() => _registry.AddSelectable<ArtifactToolRegistryTestsDummyTool>());
    }

    [Fact]
    public void AddSelectable_DuplicateGeneric_ThrowsArgumentException()
    {
        _registry.AddSelectable<ArtifactToolRegistryTestsDummyTool>();
        Assert.Throws<ArgumentException>(() => _registry.AddSelectable<ArtifactToolRegistryTestsDummyTool>());
    }

    [Fact]
    public void Add_WithSelectableType_DisallowsSelectionOfSelectable()
    {
        _registry.Add<ArtifactToolRegistryTestsDummyTool>();
        bool success = _registry.TryIdentify("ID_1234", out ArtifactToolID? artifactToolId, out string? artifactId);
        Assert.False(success);
        Assert.Null(artifactToolId);
        Assert.Null(artifactId);
    }

    [Fact]
    public void AddSelectable_WithSelectableType_AllowsSelectionOfSelectable()
    {
        _registry.AddSelectable<ArtifactToolRegistryTestsDummyTool>();
        bool success = _registry.TryIdentify("ID_1234", out ArtifactToolID? artifactToolId, out string? artifactId);
        Assert.True(success);
        Assert.Equal(s_dummyToolId, artifactToolId);
        success = _registry.TryLoad(artifactToolId!, out IArtifactTool? artifactTool);
        Assert.True(success);
        Assert.IsType<ArtifactToolRegistryTestsDummyTool>(artifactTool);
        Assert.Equal("1234", artifactId);
        artifactTool.Dispose();
    }

    [Fact]
    public void TryLoad_ValidToolId_Succeeds()
    {
        _registry.AddSelectable<ArtifactToolRegistryTestsDummyTool>();
        bool success = _registry.TryLoad(s_dummyToolId, out IArtifactTool? artifactTool);
        Assert.True(success);
        Assert.IsType<ArtifactToolRegistryTestsDummyTool>(artifactTool);
        artifactTool.Dispose();
    }

    [Fact]
    public void TryLoad_CustomToolId_Succeeds()
    {
        _registry.Add(new ArtifactToolSelectableRegistryEntry<ArtifactToolRegistryTestsDummyTool>(s_customToolId));
        bool success = _registry.TryLoad(s_customToolId, out IArtifactTool? artifactTool);
        Assert.True(success);
        Assert.IsType<ArtifactToolRegistryTestsDummyTool>(artifactTool);
        artifactTool.Dispose();
    }

    [Fact]
    public void TryLoad_InvalidToolId_Fails()
    {
        _registry.AddSelectable<ArtifactToolRegistryTestsDummyTool>();
        bool success = _registry.TryLoad(new ArtifactToolID("InvalidAssembly", "InvalidType"), out IArtifactTool? artifactTool);
        Assert.False(success);
        Assert.Null(artifactTool);
    }

    [Fact]
    public void TryIdentify_ValidKey_Succeeds()
    {
        _registry.AddSelectable<ArtifactToolRegistryTestsDummyTool>();
        bool success = _registry.TryIdentify("ID_1234", out ArtifactToolID? artifactToolId, out string? artifactId);
        Assert.True(success);
        Assert.Equal(s_dummyToolId, artifactToolId);
        Assert.Equal("1234", artifactId);
    }

    [Fact]
    public void TryIdentify_CustomToolId_ValidKey_Succeeds()
    {
        _registry.Add(new ArtifactToolSelectableRegistryEntry<ArtifactToolRegistryTestsDummyTool>(s_customToolId));
        bool success = _registry.TryIdentify("ID_1234", out ArtifactToolID? artifactToolId, out string? artifactId);
        Assert.True(success);
        Assert.Equal(s_customToolId, artifactToolId);
        Assert.Equal("1234", artifactId);
    }

    [Fact]
    public void TryIdentify_InvalidKey_Fails()
    {
        _registry.AddSelectable<ArtifactToolRegistryTestsDummyTool>();
        bool success = _registry.TryIdentify("INVALID_1234", out ArtifactToolID? artifactToolId, out string? artifactId);
        Assert.False(success);
        Assert.Null(artifactToolId);
        Assert.Null(artifactId);
    }

    [Fact]
    public void TryIdentifyAndLoad_ValidKey_Succeeds()
    {
        _registry.AddSelectable<ArtifactToolRegistryTestsDummyTool>();
        bool success = _registry.TryIdentify("ID_1234", out ArtifactToolID? artifactToolId, out string? artifactId);
        Assert.True(success);
        Assert.Equal(s_dummyToolId, artifactToolId);
        success = _registry.TryLoad(artifactToolId!, out IArtifactTool? artifactTool);
        Assert.True(success);
        Assert.IsType<ArtifactToolRegistryTestsDummyTool>(artifactTool);
        Assert.Equal("1234", artifactId);
        artifactTool.Dispose();
    }

    [Fact]
    public void TryIdentifyAndLoad_CustomToolId_ValidKey_Succeeds()
    {
        _registry.Add(new ArtifactToolSelectableRegistryEntry<ArtifactToolRegistryTestsDummyTool>(s_customToolId));
        bool success = _registry.TryIdentify("ID_1234", out ArtifactToolID? artifactToolId, out string? artifactId);
        Assert.True(success);
        Assert.Equal(s_customToolId, artifactToolId);
        success = _registry.TryLoad(artifactToolId!, out IArtifactTool? artifactTool);
        Assert.True(success);
        Assert.IsType<ArtifactToolRegistryTestsDummyTool>(artifactTool);
        Assert.Equal("1234", artifactId);
        artifactTool.Dispose();
    }

    [Fact]
    public void TryIdentifyAndLoad_InvalidKey_Fails()
    {
        _registry.AddSelectable<ArtifactToolRegistryTestsDummyTool>();
        bool success = _registry.TryIdentify("INVALID_1234", out ArtifactToolID? artifactToolId, out string? artifactId);
        Assert.False(success);
        Assert.Null(artifactToolId);
        Assert.Null(artifactId);
    }
}

internal partial class ArtifactToolRegistryTestsDummyTool : ArtifactTool, IArtifactToolSelfFactory<ArtifactToolRegistryTestsDummyTool>, IArtifactToolRegexSelector<ArtifactToolRegistryTestsDummyTool>
{
    [GeneratedRegex(@"^ID_(?<ID_GROUP>\d+)$")]
    public static partial Regex GetArtifactToolSelectorRegex();

    public static string GetArtifactToolSelectorRegexIdGroupName() => "ID_GROUP";
}
