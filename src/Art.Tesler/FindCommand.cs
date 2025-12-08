using System.CommandLine;
using Art.Common;
using Art.Common.Management;
using Art.Common.Proxies;
using Art.Tesler.Properties;

namespace Art.Tesler;

public class FindCommand : ToolCommandBase
{
    protected TimeProvider TimeProvider;

    protected Argument<List<string>> IdsArg;

    protected Option<string> ProfileFileOption;

    protected Option<bool> ListResourceOption;

    protected Option<string> ToolOption;

    protected Option<string> GroupOption;

    protected Option<bool> DetailedOption;

    public FindCommand(
        IToolLogHandlerProvider toolLogHandlerProvider,
        IArtifactToolRegistryStore pluginStore,
        IToolPropertyProvider toolPropertyProvider,
        TimeProvider timeProvider
    )
        : this(toolLogHandlerProvider, pluginStore, toolPropertyProvider, timeProvider, "find", "Execute artifact finder tools.")
    {
    }

    public FindCommand(
        IToolLogHandlerProvider toolLogHandlerProvider,
        IArtifactToolRegistryStore pluginStore,
        IToolPropertyProvider toolPropertyProvider,
        TimeProvider timeProvider,
        string name,
        string? description = null) :
        base(toolLogHandlerProvider, pluginStore, toolPropertyProvider, name, description)
    {
        TimeProvider = timeProvider;
        IdsArg = new Argument<List<string>>("ids") { HelpName = "id", Arity = ArgumentArity.OneOrMore, Description = "IDs" };
        Add(IdsArg);
        ListResourceOption = new Option<bool>("-l", "--list-resource") { Description = "List associated resources" };
        Add(ListResourceOption);
        ProfileFileOption = new Option<string>("-i", "--input") { HelpName = "file", Description = "Profile file" };
        Add(ProfileFileOption);
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
        if (profileFile == null)
        {
            ArtifactToolProfile profile = new(tool!, group, null);
            return await ExecAsync(parseResult, profile, cancellationToken).ConfigureAwait(false);
        }
        int ec = 0;
        foreach (ArtifactToolProfile profile in ArtifactToolProfileUtil.DeserializeProfilesFromFile(profileFile))
        {
            if (group != null && group != profile.Group || tool != null && tool != profile.Tool) continue;
            ec = Common.AccumulateErrorCode(await ExecAsync(parseResult, profile, cancellationToken).ConfigureAwait(false), ec);
        }
        return ec;
    }

    private async Task<int> ExecAsync(ParseResult parseResult, ArtifactToolProfile profile, CancellationToken cancellationToken)
    {
        using var arm = new InMemoryArtifactRegistrationManager();
        using var adm = new NullArtifactDataManager();
        profile = PrepareProfile(parseResult, profile);
        (bool getArtifactRetrievalTimestamps, bool getResourceRetrievalTimestamps) = GetArtifactRetrievalOptions(parseResult);
        using var tool = await GetToolAsync(profile, arm, adm, TimeProvider, getArtifactRetrievalTimestamps, getResourceRetrievalTimestamps, cancellationToken).ConfigureAwait(false);
        ArtifactToolFindProxy proxy = new(tool, ToolLogHandlerProvider.GetDefaultToolLogHandler());
        bool listResource = parseResult.GetValue(ListResourceOption);
        bool detailed = parseResult.GetValue(DetailedOption);
        foreach (string id in parseResult.GetRequiredValue(IdsArg))
        {
            IArtifactData? data = null;
            try
            {
                data = await proxy.FindAsync(id, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                PrintExceptionMessage(ex, ToolOutput);
                continue;
            }
            finally
            {
                if (data == null) PrintWarningMessage($"!! [{id}] not found", ToolOutput);
            }
            if (data != null)
            {
                await Common.DisplayAsync(data, listResource, detailed, ToolOutput).ConfigureAwait(false);
            }
        }
        return 0;
    }
}
