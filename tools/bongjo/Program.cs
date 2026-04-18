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
    private readonly Option<bool> _useApiDiffs;

    public ReviewGithubCommand() : this("review", "Produce review comments for GitHub")
    {
    }

    public ReviewGithubCommand(string name, string? description = null) : base(name, description)
    {
        _headCommit = new Option<string>("--head-commit") { Description = "Commit containing the target code", Required = true };
        Add(_headCommit);
        _baseCommit = new Option<string>("--base-commit") { Description = "Base commit to compare against" };
        Add(_baseCommit);
        _sarifFile = new Option<FileInfo>("--sarif-file") { Description = "SARIF file to ingest", Required = true };
        _sarifFile.AcceptExistingOnly();
        Add(_sarifFile);
        _repoIdOption = new Option<long>("--repo-id") { Description = "GitHub repo ID", Required = true };
        Add(_repoIdOption);
        _issueNumberOption = new Option<int>("--issue") { Description = "Issue number in repo", Required = true };
        Add(_issueNumberOption);
        _useApiDiffs = new Option<bool>("--use-api-diffs") { Description = "If true, use GitHub API to retrieve diffs" };
        Add(_useApiDiffs);
        SetAction(HandleAsync);
    }

    private async Task<int> HandleAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        string headCommit = parseResult.GetRequiredValue(_headCommit);
        string? baseCommit = parseResult.GetValue(_baseCommit);
        FileInfo sarifFileSrc = parseResult.GetRequiredValue(_sarifFile);
        long repoId = parseResult.GetRequiredValue(_repoIdOption);
        int issue = parseResult.GetRequiredValue(_issueNumberOption);
        bool useApiDiffs = parseResult.GetValue(_useApiDiffs);
        SarifFile sarifFile;
        using (var stream = sarifFileSrc.OpenRead())
        {
            sarifFile = await JsonSerializer.DeserializeAsync(stream, SourceGenerationContext.SharedContext.SarifFile, cancellationToken)
                        ?? throw new InvalidDataException($"Unexpected null json in sarif file {sarifFileSrc.FullName}");
        }
        var gitHubClient = new GitHubClient(new ProductHeaderValue("bongjo"));
        gitHubClient.Credentials = new Credentials(Environment.GetEnvironmentVariable("GITHUB_TOKEN") ?? "");
        var entriesPerLocation = new Dictionary<string, List<KeyValuePair<SarifRunResultLocation, SarifRunResult>>>();
        foreach (var run in sarifFile.Runs)
        {
            foreach (var result in run.Results)
            {
                foreach (var location in result.Locations)
                {
                    string path = Path.GetFullPath(new Uri(location.PhysicalLocation.ArtifactLocation.Uri).LocalPath);
                    if (!entriesPerLocation.TryGetValue(path, out var list))
                    {
                        entriesPerLocation.Add(path, list = []);
                    }
                    list.Add(new KeyValuePair<SarifRunResultLocation, SarifRunResult>(location, result));
                }
            }
        }
        List<TargetedComment> comments = [];
        if (useApiDiffs)
        {
            var files = await gitHubClient.PullRequest.Files(repoId, issue);
            foreach (var file in files)
            {
                await ProcessDiffForFile(new StringReader(file.Patch), comments, entriesPerLocation, Path.GetFullPath(file.FileName));
            }
        }
        else
        {
            if (baseCommit == null)
            {
                Console.Error.WriteLine($"At least one of {_useApiDiffs.Name} or {_baseCommit.Name} must be supplied");
                return 1;
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
            await ProcessDiff(process.StandardOutput, comments, entriesPerLocation);
            await process.WaitForExitAsync(cancellationToken);
        }
        /*foreach (var run in sarifFile.Runs)
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
                                    out int overlapLine,
                                    out _))
                            {
                                comments.Add(new TargetedComment(
                                    localPath,
                                    overlapLine - range.Line + range.DiffDelta,
                                    overlapLine,
                                    result.Message.Text,
                                    result.RuleId));
                            }
                        }
                    }
                }
            }
        }*/
        var reviewComments = await gitHubClient.PullRequest.ReviewComment.GetAll(repoId, issue);
        var reviewCommentsByKey = reviewComments
            .GroupBy(static v => new CommentKey(v.Path, v.OriginalPosition ?? 0))
            .ToDictionary(static v => v.Key, static v2 => v2.ToList());
        foreach (var comment in comments)
        {
            var commentKey = new CommentKey(comment.File, comment.PatchLine);
            if (reviewCommentsByKey.TryGetValue(commentKey, out var existingReviewComments))
            {
                bool skipComment = false;
                foreach (var existingReviewComment in existingReviewComments)
                {
                    if (!string.IsNullOrWhiteSpace(comment.RuleId) && existingReviewComment.Body.Contains(comment.RuleId))
                    {
                        skipComment = true;
                        break;
                    }
                }
                if (skipComment)
                {
                    Console.WriteLine($"Skipping comment for {commentKey.File} diff line {commentKey.Line} rule {comment.RuleId}");
                    continue;
                }
            }
            Console.WriteLine(comment);
            string commentText = $"{comment.Message} ({comment.RuleId})";
            await gitHubClient.PullRequest.ReviewComment.Create(repoId, issue, new PullRequestReviewCommentCreate(commentText, headCommit, comment.File, comment.PatchLine));
        }
        return 0;
    }

    internal record TargetedComment(string File, int PatchLine, int Line, string Message, string RuleId);

    private record struct CommentKey(string File, int Line);

    internal static async Task ProcessDiff(
        StreamReader textReader,
        List<TargetedComment> comments,
        IReadOnlyDictionary<string, List<KeyValuePair<SarifRunResultLocation, SarifRunResult>>> entriesPerLocation)
    {
        // look exclusively for added code
        string? line = await textReader.ReadLineAsync();
        while (line != null)
        {
            if (!line.StartsWith("+++") || line.EndsWith("/dev/null"))
            {
                line = await textReader.ReadLineAsync();
                continue;
            }
            if (GetPlusLineFormat().Match(line) is not { Success: true } lineMatch)
            {
                throw new InvalidDataException($"Unmatched +++ section: {line}");
            }
            string fullPath = Path.GetFullPath(lineMatch.Groups["path"].Value);
            line = await ProcessDiffForFile(textReader, comments, entriesPerLocation, fullPath) ?? await textReader.ReadLineAsync();
        }
    }

    internal static async Task<string?> ProcessDiffForFile(
        TextReader textReader,
        List<TargetedComment> comments,
        IReadOnlyDictionary<string, List<KeyValuePair<SarifRunResultLocation, SarifRunResult>>> entriesPerLocation,
        string fullPath, int lineOffset = 0)
    {
        int? lineNumber = null;
        int localLineNumber = 1;
        while (await textReader.ReadLineAsync() is { } line)
        {
            if (line.StartsWith("@@"))
            {
                if (GetDoubleAtLineFormat().Match(line) is not { Success: true } cmpLineMatch)
                {
                    throw new InvalidDataException($"Unmatched @@ section: {line}");
                }
                lineNumber = int.Parse(cmpLineMatch.Groups["plusStart"].Value, CultureInfo.InvariantCulture);
                Console.WriteLine($"{lineNumber}/{localLineNumber}:{line}");
            }
            else if (line.StartsWith('+') || line.StartsWith('-') || line.StartsWith(' '))
            {
                Console.WriteLine($"{lineNumber}/{localLineNumber}:{line}");
                if (entriesPerLocation.TryGetValue(fullPath, out var entries))
                {
                    foreach (var entry in entries)
                    {
                        if (lineNumber == entry.Key.PhysicalLocation.Region.StartLine)
                        {
                            string localPath = Path.GetRelativePath(Path.GetFullPath("."), fullPath);
                            if (new HashSet<char> { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }.SetEquals(['/', '\\']))
                            {
                                localPath = localPath.Replace('\\', '/');
                            }
                            comments.Add(new TargetedComment(
                                localPath,
                                localLineNumber,
                                (int)lineNumber,
                                entry.Value.Message.Text,
                                entry.Value.RuleId));
                        }
                    }
                }
                localLineNumber++;
                if (!line.StartsWith('-') && lineNumber != null)
                {
                    lineNumber++;
                }
            }
            else
            {
                return line;
            }
        }
        return null;
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

internal record struct FixedRange(int Line, int Length, int DiffDelta) : IComparable<FixedRange>
{
    public bool OverlapsLines(int startLine, int lineCount, out int matchLine, out int matchLineCount)
    {
        int maxSharedStart = Math.Max(startLine, Line);
        int minSharedEnd = Math.Min(startLine + lineCount, Line + Length);
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
        int indexComparison = Line.CompareTo(other.Line);
        if (indexComparison != 0)
        {
            return indexComparison;
        }
        return Length.CompareTo(other.Length);
    }
}
