using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using Art.Common;
using Art.Common.Management;
using Art.Common.Proxies;
using Art.Tesler.Properties;

namespace Art.Tesler;

public class DumpCommand : ToolCommandBase
{
    protected ITeslerDataProvider DataProvider;

    protected ITeslerRegistrationProvider RegistrationProvider;

    protected TimeProvider TimeProvider;

    protected Option<string> HashOption;

    protected Option<bool> NoDatabaseOption;

    protected Option<string> ProfileFileOption;

    protected Option<string> ToolOption;

    protected Option<string> GroupOption;

    public DumpCommand(
        IToolLogHandlerProvider toolLogHandlerProvider,
        IArtifactToolRegistryStore pluginStore,
        IToolPropertyProvider toolPropertyProvider,
        ITeslerDataProvider dataProvider,
        ITeslerRegistrationProvider registrationProvider,
        TimeProvider timeProvider)
        : this(toolLogHandlerProvider, pluginStore, toolPropertyProvider, dataProvider, registrationProvider, timeProvider, "dump", "Execute artifact dump tools.")
    {
    }

    public DumpCommand(
        IToolLogHandlerProvider toolLogHandlerProvider,
        IArtifactToolRegistryStore pluginStore,
        IToolPropertyProvider toolPropertyProvider,
        ITeslerDataProvider dataProvider,
        ITeslerRegistrationProvider registrationProvider,
        TimeProvider timeProvider,
        string name,
        string? description = null)
        : base(toolLogHandlerProvider, pluginStore, toolPropertyProvider, name, description)
    {
        DataProvider = dataProvider;
        DataProvider.Initialize(this);
        RegistrationProvider = registrationProvider;
        RegistrationProvider.Initialize(this);
        TimeProvider = timeProvider;
        HashOption = new Option<string>("-h", "--hash") { Description = $"Checksum algorithm ({Common.ChecksumAlgorithms})", DefaultValueFactory = static _ => Common.DefaultChecksumAlgorithm };
        Add(HashOption);
        NoDatabaseOption = new Option<bool>("--no-database") { Description = "Don't use database to track resources" };
        Add(NoDatabaseOption);
        ProfileFileOption = new Option<string>("-i", "--input") { HelpName = "file", Description = "Profile file" };
        Add(ProfileFileOption);
        ToolOption = new Option<string>("-t", "--tool") { HelpName = "name", Description = "Tool to use or filter profiles by" };
        Add(ToolOption);
        GroupOption = new Option<string>("-g", "--group") { HelpName = "name", Description = "Group to use or filter profiles by" };
        Add(GroupOption);
        Validators.Add(v =>
        {
            if (v.GetValue(ProfileFileOption) == null && v.GetValue(ToolOption) == null)
            {
                v.AddError($"At least one of {ProfileFileOption.Aliases.First()} or {ToolOption.Aliases.First()} must be passed.");
            }
        });
    }

    protected override async Task<int> RunAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        using var adm = DataProvider.CreateArtifactDataManager(parseResult);
        if (parseResult.GetValue(NoDatabaseOption))
        {
            InMemoryArtifactRegistrationManager arm = new();
            return await RunAsync(parseResult, adm, arm, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            using var arm = RegistrationProvider.CreateArtifactRegistrationManager(parseResult);
            return await RunAsync(parseResult, adm, arm, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task<int> RunAsync(ParseResult parseResult, IArtifactDataManager adm, IArtifactRegistrationManager arm, CancellationToken cancellationToken)
    {
        ChecksumSource? checksumSource;
        string? hash = parseResult.GetValue(HashOption);
        hash = string.Equals(hash, "none", StringComparison.InvariantCultureIgnoreCase) ? null : hash;
        if (hash == null)
        {
            checksumSource = null;
        }
        else
        {
            if (!ChecksumSource.DefaultSources.TryGetValue(hash, out checksumSource))
            {
                PrintErrorMessage(Common.GetInvalidHashMessage(hash), ToolOutput);
                return 2;
            }
        }
        string? profileFile = parseResult.GetValue(ProfileFileOption);
        string? tool = parseResult.GetValue(ToolOption);
        string? group = parseResult.GetValue(GroupOption);
        (bool getArtifactRetrievalTimestamps, bool getResourceRetrievalTimestamps) = GetArtifactRetrievalOptions(parseResult);
        if (profileFile == null)
        {
            return await ExecAsync(parseResult, new ArtifactToolProfile(tool!, group, null), arm, adm, checksumSource, getArtifactRetrievalTimestamps, getResourceRetrievalTimestamps, cancellationToken).ConfigureAwait(false);
        }
        int ec = 0;
        foreach (ArtifactToolProfile profile in ArtifactToolProfileUtil.DeserializeProfilesFromFile(profileFile))
        {
            if (group != null && group != profile.Group || tool != null && tool != profile.Tool) continue;
            ec = Common.AccumulateErrorCode(await ExecAsync(parseResult, profile, arm, adm, checksumSource, getArtifactRetrievalTimestamps, getResourceRetrievalTimestamps, cancellationToken).ConfigureAwait(false), ec);
        }
        return ec;
    }

    private async Task<int> ExecAsync(
        ParseResult parseResult,
        ArtifactToolProfile profile,
        IArtifactRegistrationManager arm,
        IArtifactDataManager adm,
        ChecksumSource? checksumSource,
        bool getArtifactRetrievalTimestamps,
        bool getResourceRetrievalTimestamps,
        CancellationToken cancellationToken)
    {
        ArtifactToolDumpOptions options = new(ChecksumSource: checksumSource);
        profile = PrepareProfile(parseResult, profile);
        using var tool = await GetToolAsync(profile, arm, adm, TimeProvider, getArtifactRetrievalTimestamps, getResourceRetrievalTimestamps, cancellationToken).ConfigureAwait(false);
        ArtifactToolDumpProxy dProxy = new(tool, options, ToolLogHandlerProvider.GetDefaultToolLogHandler());
        await dProxy.DumpAsync(cancellationToken).ConfigureAwait(false);
        return 0;
    }
}
