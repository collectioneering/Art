using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using Art.Common;
using Art.Common.Management;
using Art.Common.Proxies;
using Art.Tesler.Properties;

namespace Art.Tesler;

public class ListCommand : ToolCommandBase
{
    protected TimeProvider TimeProvider;

    protected Option<string> ProfileFileOption;

    protected Option<bool> ListResourceOption;

    protected Option<string> ToolOption;

    protected Option<string> GroupOption;

    protected Option<bool> DetailedOption;

    public ListCommand(
        IToolLogHandlerProvider toolLogHandlerProvider,
        IArtifactToolRegistryStore pluginStore,
        IToolPropertyProvider toolPropertyProvider,
        TimeProvider timeProvider
    )
        : this(toolLogHandlerProvider, pluginStore, toolPropertyProvider, timeProvider, "list", "Execute artifact list tools.")
    {
    }

    public ListCommand(
        IToolLogHandlerProvider toolLogHandlerProvider,
        IArtifactToolRegistryStore pluginStore,
        IToolPropertyProvider toolPropertyProvider,
        TimeProvider timeProvider,
        string name,
        string? description = null)
        : base(toolLogHandlerProvider, pluginStore, toolPropertyProvider, name, description)
    {
        TimeProvider = timeProvider;
        ProfileFileOption = new Option<string>("-i", "--input") { HelpName = "file", Description = "Profile file" };
        Add(ProfileFileOption);
        ListResourceOption = new Option<bool>("-l", "--list-resource") { Description = "List associated resources" };
        Add(ListResourceOption);
        ToolOption = new Option<string>("-t", "--tool") { HelpName = "name", Description = "Tool to use or filter profiles by" };
        Add(ToolOption);
        GroupOption = new Option<string>("-g", "--group") { HelpName = "name", Description = "Group to use or filter profiles by" };
        Add(GroupOption);
        DetailedOption = new Option<bool>("--detailed") { Description = "Show detailed information on entries" };
        Add(DetailedOption);
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
        string? profileFile = parseResult.GetValue(ProfileFileOption);
        string? tool = parseResult.GetValue(ToolOption);
        string? group = parseResult.GetValue(GroupOption);
        (bool getArtifactRetrievalTimestamps, bool getResourceRetrievalTimestamps) = GetArtifactRetrievalOptions(parseResult);
        if (profileFile == null) return await ExecAsync(parseResult, new ArtifactToolProfile(tool!, group, null), getArtifactRetrievalTimestamps, getResourceRetrievalTimestamps, cancellationToken).ConfigureAwait(false);
        int ec = 0;
        foreach (ArtifactToolProfile profile in ArtifactToolProfileUtil.DeserializeProfilesFromFile(profileFile))
        {
            if (group != null && group != profile.Group || tool != null && tool != profile.Tool) continue;
            ec = Common.AccumulateErrorCode(await ExecAsync(parseResult, profile, getArtifactRetrievalTimestamps, getResourceRetrievalTimestamps, cancellationToken).ConfigureAwait(false), ec);
        }
        return ec;
    }

    private async Task<int> ExecAsync(
        ParseResult parseResult,
        ArtifactToolProfile profile,
        bool getArtifactRetrievalTimestamps,
        bool getResourceRetrievalTimestamps,
        CancellationToken cancellationToken)
    {
        using var arm = new InMemoryArtifactRegistrationManager();
        using var adm = new NullArtifactDataManager();
        profile = PrepareProfile(parseResult, profile);
        using var tool = await GetToolAsync(profile, arm, adm, TimeProvider, getArtifactRetrievalTimestamps, getResourceRetrievalTimestamps, cancellationToken).ConfigureAwait(false);
        ArtifactToolListOptions options = new();
        ArtifactToolListProxy proxy = new(tool, options, ToolLogHandlerProvider.GetDefaultToolLogHandler());
        bool listResource = parseResult.GetValue(ListResourceOption);
        bool detailed = parseResult.GetValue(DetailedOption);
        await foreach (IArtifactData data in proxy.ListAsync(cancellationToken).ConfigureAwait(false))
        {
            await Common.DisplayAsync(data, listResource, detailed, ToolOutput).ConfigureAwait(false);
        }
        return 0;
    }
}
