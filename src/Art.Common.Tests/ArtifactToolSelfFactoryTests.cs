
namespace Art.Common.Tests;

public class ArtifactToolSelfFactoryTests
{
    private static readonly ArtifactToolID s_dummyToolId = new("Art.Common.Tests", "Art.Common.Tests.ArtifactToolSelfFactoryTestsDummyTool");

    [Fact]
    public void GetArtifactToolId_MatchesExpected()
    {
        var id = GetArtifactToolId<ArtifactToolSelfFactoryTestsDummyTool>();
        Assert.Equal(s_dummyToolId, id);
    }

    [Fact]
    public void CreateArtifactTool_MatchesType()
    {
        var tool = CreateArtifactTool<ArtifactToolSelfFactoryTestsDummyTool>();
        Assert.IsType<ArtifactToolSelfFactoryTestsDummyTool>(tool);
    }

    private static ArtifactToolID GetArtifactToolId<T>() where T : IArtifactToolSelfFactory<T>, IArtifactTool, new()
    {
        return T.GetArtifactToolId();
    }

    private static IArtifactTool CreateArtifactTool<T>() where T : IArtifactToolSelfFactory<T>, IArtifactTool, new()
    {
        return T.CreateArtifactTool();
    }
}

internal class ArtifactToolSelfFactoryTestsDummyTool : ArtifactTool, IArtifactToolSelfFactory<ArtifactToolSelfFactoryTestsDummyTool>
{
}
