﻿using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Art.Common;

namespace Art.Tesler.Tests;

internal partial class ExampleArtifactFindTool : ArtifactTool,
    IArtifactToolSelfFactory<ExampleArtifactFindTool>,
    IArtifactToolRegexSelector<ExampleArtifactFindTool>,
    IArtifactFindTool
{
    [GeneratedRegex(@"https://example\.com/(?<id>\d+)(?:$|/)")]
    public static partial Regex GetArtifactToolSelectorRegex();

    public static string GetArtifactToolSelectorRegexIdGroupName() => "id";

    public Task<IArtifactData?> FindAsync(string id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IArtifactData?>(CreateData(id));
    }
}