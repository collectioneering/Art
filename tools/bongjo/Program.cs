using System.CommandLine;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Octokit;

var rootCommand = new RootCommand("Support for sarif files and github review comments");
rootCommand.Add(new ReviewGithubCommand());
var parseResult = rootCommand.Parse(args);
parseResult.InvocationConfiguration.Output = Console.Error;
parseResult.InvocationConfiguration.Error = Console.Error;
return await parseResult.InvokeAsync();

public sealed partial class ReviewGithubCommand : Command
{
    [GeneratedRegex(@"^\+\+\+\s*(?<key>\w+/)(?<path>.+)$")]
    private static partial Regex GetPlusLineFormat();

    [GeneratedRegex(@"^@@\s*(?:-(?<minusStart>\d+),(?<minusLength>\d+))?\s*\+(?<plusStart>\d+),(?<plusLength>\d+)\s*@@")]
    private static partial Regex GetDoubleAtLineFormat();

    private readonly Option<string> _headCommit;
    private readonly Option<string> _baseCommit;
    private readonly Option<FileInfo> _sarifFile;
    private readonly Option<long> _repoIdOption;
    private readonly Option<int> _issueNumberOption;

    public ReviewGithubCommand() : this("review", "Produce review comments for GitHub")
    {
    }

    public ReviewGithubCommand(string name, string? description = null) : base(name, description)
    {
        _headCommit = new Option<string>("--head-commit") { Description = "Commit containing the target code", Required = true };
        Add(_headCommit);
        _baseCommit = new Option<string>("--base-commit") { Description = "Base commit to compare against", Required = true };
        Add(_baseCommit);
        _sarifFile = new Option<FileInfo>("--sarif-file") { Description = "SARIF file to ingest", Required = true };
        _sarifFile.AcceptExistingOnly();
        Add(_sarifFile);
        _repoIdOption = new Option<long>("--repo-id") { Description = "GitHub repo ID", Required = true };
        Add(_repoIdOption);
        _issueNumberOption = new Option<int>("--issue") { Description = "Issue number in repo", Required = true };
        Add(_issueNumberOption);
        SetAction(HandleAsync);
    }

    private async Task<int> HandleAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        string headCommit = parseResult.GetRequiredValue(_headCommit);
        string baseCommit = parseResult.GetRequiredValue(_baseCommit);
        FileInfo sarifFileSrc = parseResult.GetRequiredValue(_sarifFile);
        long repoId = parseResult.GetRequiredValue(_repoIdOption);
        int issue = parseResult.GetRequiredValue(_issueNumberOption);
        SarifFile sarifFile;
        using (var stream = sarifFileSrc.OpenRead())
        {
            sarifFile = await JsonSerializer.DeserializeAsync(stream, SourceGenerationContext.SharedContext.SarifFile, cancellationToken)
                        ?? throw new InvalidDataException($"Unexpected null json in sarif file {sarifFileSrc.FullName}");
        }
        var psi = new ProcessStartInfo("git") { UseShellExecute = false, RedirectStandardOutput = true };
        psi.ArgumentList.Add("diff");
        psi.ArgumentList.Add(baseCommit);
        psi.ArgumentList.Add(headCommit);
        var process = Process.Start(psi);
        if (process == null)
        {
            throw new InvalidOperationException();
        }
        Dictionary<string, List<FixedRange>> ranges = new();
        foreach (var list in ranges.Values)
        {
            list.Sort();
        }
        await ProcessDiff(process.StandardOutput, ranges);
        await process.WaitForExitAsync(cancellationToken);
        List<TargetedComment> comments = [];
        foreach (var run in sarifFile.Runs)
        {
            foreach (var result in run.Results)
            {
                foreach (var location in result.Locations)
                {
                    string path = Path.GetFullPath(new Uri(location.PhysicalLocation.ArtifactLocation.Uri).LocalPath);
                    if (ranges.TryGetValue(path, out var fileRanges))
                    {
                        string localPath = Path.GetRelativePath(Path.GetFullPath("."), path);
                        if (new HashSet<char> { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }.SetEquals(['/', '\\']))
                        {
                            localPath = localPath.Replace('\\', '/');
                        }
                        foreach (var range in fileRanges)
                        {
                            if (range.OverlapsLines(
                                    location.PhysicalLocation.Region.StartLine,
                                    location.PhysicalLocation.Region.EndLine - location.PhysicalLocation.Region.StartLine + 1,
                                    out int overlapColumn,
                                    out _))
                            {
                                comments.Add(new TargetedComment(localPath, overlapColumn, result.Message.Text, result.RuleId));
                            }
                        }
                    }
                }
            }
        }
        var gitHubClient = new GitHubClient(new ProductHeaderValue("bongjo"));
        gitHubClient.Credentials = new Credentials(Environment.GetEnvironmentVariable("GITHUB_TOKEN") ?? "");
        foreach (var comment in comments)
        {
            string commentText = $"{comment.Message} ({comment.RuleId})";
            await gitHubClient.PullRequest.ReviewComment.Create(repoId, issue, new PullRequestReviewCommentCreate(commentText, headCommit, comment.File, comment.Line));
        }
        return 0;
    }

    private record TargetedComment(string File, int Line, string Message, string RuleId);

    private async Task ProcessDiff(StreamReader sr, Dictionary<string, List<FixedRange>> ranges)
    {
        // look exclusively for added code
        string? line;
        while ((line = await sr.ReadLineAsync()) != null)
        {
            if (!line.StartsWith("+++") || line.EndsWith("/dev/null"))
            {
                continue;
            }
            if (GetPlusLineFormat().Match(line) is not { Success: true } lineMatch)
            {
                throw new InvalidDataException($"Unmatched +++ section: {line}");
            }
            while ((line = await sr.ReadLineAsync()) != null)
            {
                if (line.StartsWith('+') || line.StartsWith('-') || line.StartsWith(' '))
                {
                    continue;
                }
                if (!line.StartsWith("@@"))
                {
                    break;
                }
                if (GetDoubleAtLineFormat().Match(line) is not { Success: true } cmpLineMatch)
                {
                    throw new InvalidDataException($"Unmatched @@ section: {line}");
                }
                string fullPath = Path.GetFullPath(lineMatch.Groups["path"].Value);
                int start = int.Parse(cmpLineMatch.Groups["plusStart"].Value, CultureInfo.InvariantCulture);
                int count = int.Parse(cmpLineMatch.Groups["plusLength"].Value, CultureInfo.InvariantCulture);
                if (!ranges.TryGetValue(fullPath, out var list))
                {
                    ranges.Add(fullPath, list = []);
                }
                list.Add(new FixedRange(start, count));
            }
        }
    }
}

public record SarifFile(
    [property: JsonPropertyName("$schema")] string Schema,
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("runs")] IReadOnlyList<SarifRun> Runs
);

public record SarifRun(
    [property: JsonPropertyName("results")] IReadOnlyList<SarifRunResult> Results
);

public record SarifRunResult(
    [property: JsonPropertyName("ruleId")] string RuleId,
    [property: JsonPropertyName("ruleIndex")] int RuleIndex,
    [property: JsonPropertyName("level")] string Level,
    [property: JsonPropertyName("message")] SarifRunResultMessage Message,
    [property: JsonPropertyName("locations")] IReadOnlyList<SarifRunResultLocation> Locations,
    [property: JsonPropertyName("partialFingerprints")] IReadOnlyDictionary<string, string> PartialFingerprints,
    [property: JsonPropertyName("properties")] SarifRunResultProperties Properties
);

public record SarifRunResultMessage(
    [property: JsonPropertyName("text")] string Text
);

public record SarifRunResultLocation(
    [property: JsonPropertyName("physicalLocation")] SarifRunResultLocationPhysicalLocation PhysicalLocation
);

public record SarifRunResultLocationPhysicalLocation(
    [property: JsonPropertyName("artifactLocation")] SarifRunResultLocationPhysicalLocationArtifactLocation ArtifactLocation,
    [property: JsonPropertyName("region")] SarifRunResultLocationPhysicalLocationRegion Region
);

public record SarifRunResultLocationPhysicalLocationArtifactLocation(
    [property: JsonPropertyName("uri")] string Uri,
    [property: JsonPropertyName("index")] int Index
);

public record SarifRunResultLocationPhysicalLocationRegion(
    [property: JsonPropertyName("startLine")] int StartLine,
    [property: JsonPropertyName("startColumn")] int StartColumn,
    [property: JsonPropertyName("endLine")] int EndLine,
    [property: JsonPropertyName("endColumn")] int EndColumn,
    [property: JsonPropertyName("charOffset")] int CharOffset,
    [property: JsonPropertyName("charLength")] int CharLength
);

public record SarifRunResultProperties(
    [property: JsonPropertyName("tags")] IReadOnlyList<string> Tags
);

internal partial class SourceGenerationContext
{
    internal static SourceGenerationContextImpl SharedContext => SourceGenerationContextImpl.s_context;

    [JsonSerializable(typeof(SarifFile))]
    internal partial class SourceGenerationContextImpl : JsonSerializerContext
    {
        static SourceGenerationContextImpl()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            options.Converters.Add(new JsonStringEnumConverter());
            s_context = new SourceGenerationContextImpl(options);
        }

        internal static readonly SourceGenerationContextImpl s_context;
    }
}

internal record struct FixedRange(int Index, int Length) : IComparable<FixedRange>
{
    public bool OverlapsLines(int startLine, int lineCount, out int matchLine, out int matchLineCount)
    {
        int maxSharedStart = Math.Max(startLine, Index);
        int minSharedEnd = Math.Min(startLine + lineCount, Index + Length);
        if (maxSharedStart < minSharedEnd)
        {
            matchLine = maxSharedStart;
            matchLineCount = minSharedEnd - maxSharedStart;
            return true;
        }
        else
        {
            matchLine = 0;
            matchLineCount = 0;
            return false;
        }
    }

    public int CompareTo(FixedRange other)
    {
        int indexComparison = Index.CompareTo(other.Index);
        if (indexComparison != 0)
        {
            return indexComparison;
        }
        return Length.CompareTo(other.Length);
    }
}
