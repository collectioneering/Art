using System.CommandLine;
using Art.Common;
using Art.Common.Proxies;
using Art.Tesler.Properties;

namespace Art.Tesler;

public class DumpCommand : ArcDumpCommandBase
{
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
        : base(toolLogHandlerProvider, pluginStore, toolPropertyProvider, dataProvider, registrationProvider, timeProvider, name, description)
    {
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

    protected override IReadOnlyList<ArtifactToolProfile> GetProfiles(ParseResult parseResult)
    {
        List<ArtifactToolProfile> profiles = [];
        string? profileFileValue = parseResult.GetValue(ProfileFileOption);
        string? toolValue = parseResult.GetValue(ToolOption);
        string? groupValue = parseResult.GetValue(GroupOption);
        if (profileFileValue == null)
        {
            profiles.Add(new ArtifactToolProfile(toolValue!, groupValue, null));
        }
        else
        {
            foreach (ArtifactToolProfile profile in ArtifactToolProfileUtil.DeserializeProfilesFromFile(profileFileValue))
            {
                if (groupValue != null && groupValue != profile.Group || toolValue != null && toolValue != profile.Tool) continue;
                profiles.Add(profile);
            }
        }
        return profiles;
    }

    protected override ArtifactToolDumpOptions GetArtifactToolDumpOptions(ParseResult parseResult, ChecksumSource? checksumSource)
    {
        return new ArtifactToolDumpOptions(ChecksumSource: checksumSource);
    }
}
