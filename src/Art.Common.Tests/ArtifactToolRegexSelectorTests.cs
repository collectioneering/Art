using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Art.Common.Tests;

public class ArtifactToolRegexSelectorTests
{
    private static readonly ArtifactToolID s_dummyToolId = new("Art.Common.Tests", "Art.Common.Tests.ArtifactToolRegexSelectorTestsDummyTool");

    [Fact]
    public void TryIdentify_ValidKey_Succeeds()
    {
        bool success = TryIdentify<ArtifactToolRegexSelectorTestsDummyTool>("ID_1234", out ArtifactToolID? artifactToolId, out string? artifactId);
        Assert.True(success);
        Assert.Equal(s_dummyToolId, artifactToolId);
        Assert.Equal("1234", artifactId);
    }

    [Fact]
    public void TryIdentify_InvalidKey_Fails()
    {
        bool success = TryIdentify<ArtifactToolRegexSelectorTestsDummyTool>("INVALID_1234", out ArtifactToolID? artifactToolId, out string? artifactId);
        Assert.False(success);
        Assert.Null(artifactToolId);
        Assert.Null(artifactId);
    }

    private static bool TryIdentify<T>(string key, [NotNullWhen(true)] out ArtifactToolID? artifactToolId, [NotNullWhen(true)] out string? artifactId) where T : IArtifactToolSelector<string>
    {
        return T.TryIdentify(key, out artifactToolId, out artifactId);
    }
}

internal partial class ArtifactToolRegexSelectorTestsDummyTool : ArtifactTool, IArtifactToolSelfFactory<ArtifactToolRegexSelectorTestsDummyTool>, IArtifactToolRegexSelector<ArtifactToolRegexSelectorTestsDummyTool>
{
    [GeneratedRegex(@"^ID_(?<ID_GROUP>\d+)$")]
    public static partial Regex GetArtifactToolSelectorRegex();

    public static string GetArtifactToolSelectorRegexIdGroupName() => "ID_GROUP";
}
