using NUnit.Framework;

namespace Art.Common.Tests;

public class ArtifactToolSelfFactoryTests
{
    private static readonly ArtifactToolID s_dummyToolId = new("Art.Common.Tests", "Art.Common.Tests.ArtifactToolSelfFactoryTestsDummyTool");

    [Test]
    public void GetArtifactToolId_MatchesExpected()
    {
        var id = GetArtifactToolId<ArtifactToolSelfFactoryTestsDummyTool>();
        Assert.That(id, Is.EqualTo(s_dummyToolId));
    }

    [Test]
    public void CreateArtifactTool_MatchesType()
    {
        var tool = CreateArtifactTool<ArtifactToolSelfFactoryTestsDummyTool>();
        Assert.That(tool, Is.InstanceOf<ArtifactToolSelfFactoryTestsDummyTool>());
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
