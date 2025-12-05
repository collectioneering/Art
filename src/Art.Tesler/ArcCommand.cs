using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using Art.Common;
using Art.Common.Management;
using Art.Common.Proxies;
using Art.Tesler.Profiles;
using Art.Tesler.Properties;

namespace Art.Tesler;

public class ArcCommand : ToolCommandBase
{
    protected ITeslerDataProvider DataProvider;

    protected ITeslerRegistrationProvider RegistrationProvider;

    protected TimeProvider TimeProvider;

    protected IProfileResolver ProfileResolver;

    protected Option<string> HashOption;

    protected Argument<List<string>> ProfileFilesArg;

    protected Option<ResourceUpdateMode> UpdateOption;

    protected Option<bool> FullOption;

    protected Option<ArtifactSkipMode> SkipOption;

    protected Option<bool> FastExitOption;

    protected Option<bool> NullOutputOption;

    public ArcCommand(
        IToolLogHandlerProvider toolLogHandlerProvider,
        IArtifactToolRegistryStore pluginStore,
        IToolPropertyProvider toolPropertyProvider,
        ITeslerDataProvider dataProvider,
        ITeslerRegistrationProvider registrationProvider,
        TimeProvider timeProvider,
        IProfileResolver profileResolver)
        : this(toolLogHandlerProvider, pluginStore, toolPropertyProvider, dataProvider, registrationProvider, timeProvider, profileResolver, "arc", "Execute archival artifact tools.")
    {
    }

    public ArcCommand(
        IToolLogHandlerProvider toolLogHandlerProvider,
        IArtifactToolRegistryStore pluginStore,
        IToolPropertyProvider toolPropertyProvider,
        ITeslerDataProvider dataProvider,
        ITeslerRegistrationProvider registrationProvider,
        TimeProvider timeProvider,
        IProfileResolver profileResolver,
        string name,
        string? description = null)
        : base(toolLogHandlerProvider, pluginStore, toolPropertyProvider, name, description)
    {
        DataProvider = dataProvider;
        DataProvider.Initialize(this);
        RegistrationProvider = registrationProvider;
        RegistrationProvider.Initialize(this);
        TimeProvider = timeProvider;
        ProfileResolver = profileResolver;
        HashOption = new Option<string>( "-h", "--hash" ) { Description = $"Checksum algorithm ({Common.ChecksumAlgorithms})", DefaultValueFactory = static _ => Common.DefaultChecksumAlgorithm };
        Add(HashOption);
        ProfileFilesArg = new Argument<List<string>>("profile") { HelpName = "profile", Arity = ArgumentArity.OneOrMore, Description =  "Profile file(s)"};
        Add(ProfileFilesArg);
        UpdateOption = new Option<ResourceUpdateMode>("-u", "--update" ) { HelpName = "mode", Description = $"Resource update mode ({Common.ResourceUpdateModes})"};
        UpdateOption.DefaultValueFactory = static _ => (ResourceUpdateMode.ArtifactHard);
        Add(UpdateOption);
        FullOption = new Option<bool>("-f", "--full" ){Description = "Only process full artifacts"};
        Add(FullOption);
        SkipOption = new Option<ArtifactSkipMode>("-s", "--skip" ){Description = $"Skip artifacts ({Common.ArtifactSkipModes})"};
        SkipOption.HelpName = "mode";
        SkipOption.DefaultValueFactory = static _ => ArtifactSkipMode.None;
        Add(SkipOption);
        FastExitOption = new Option<bool>( "-z", "--fast-exit" ){Description = $"Equivalent to -s/--skip {nameof(ArtifactSkipMode.FastExit)}"};
        Add(FastExitOption);
        NullOutputOption = new Option<bool>( "--null-output" ){Description = "Send resources to the void"};
        Add(NullOutputOption);
    }

    protected override async Task<int> RunAsync(ParseResult parseResult, CancellationToken cancellationToken)
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
        ResourceUpdateMode update = parseResult.GetValue(UpdateOption);
        bool full = parseResult.GetValue(FullOption);
        ArtifactSkipMode skip = parseResult.GetValue(SkipOption);
        bool fastExit = parseResult.GetValue(FastExitOption);
        bool nullOutput = parseResult.GetValue(NullOutputOption);
        ArtifactToolDumpOptions options = new(update, !full, fastExit ? ArtifactSkipMode.FastExit : skip, checksumSource);
        using var adm = nullOutput ? new NullArtifactDataManager() : DataProvider.CreateArtifactDataManager(parseResult);
        using var arm = RegistrationProvider.CreateArtifactRegistrationManager(parseResult);
        IToolLogHandler l = ToolLogHandlerProvider.GetDefaultToolLogHandler();
        List<ArtifactToolProfile> profiles = new();
        foreach (string profileFile in parseResult.GetRequiredValue(ProfileFilesArg))
        {
            ResolveAndAddProfiles(ProfileResolver, profiles, profileFile);
        }

        (bool getArtifactRetrievalTimestamps, bool getResourceRetrievalTimestamps) = GetArtifactRetrievalOptions(parseResult);
        foreach (ArtifactToolProfile profile in PrepareProfiles(parseResult, profiles))
        {
            using var tool = await GetToolAsync(profile, arm, adm, TimeProvider, getArtifactRetrievalTimestamps, getResourceRetrievalTimestamps, cancellationToken).ConfigureAwait(false);
            await new ArtifactToolDumpProxy(tool, options, l).DumpAsync(cancellationToken).ConfigureAwait(false);
        }

        return 0;
    }
}
