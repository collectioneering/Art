using System.CommandLine;
using System.CommandLine.Invocation;
using Art.Common;
using Art.Common.Management;
using Art.Common.Proxies;
using Art.Tesler.Profiles;
using Art.Tesler.Properties;

namespace Art.Tesler;

public class StreamCommand : ToolCommandBase
{
    protected TimeProvider TimeProvider;

    protected IProfileResolver ProfileResolver;

    protected Argument<string> ProfileFileArg;

    public StreamCommand(
        IToolLogHandlerProvider toolLogHandlerProvider,
        IArtifactToolRegistryStore pluginStore,
        IToolPropertyProvider toolPropertyProvider,
        TimeProvider timeProvider,
        IProfileResolver profileResolver)
        : this(toolLogHandlerProvider, pluginStore, toolPropertyProvider, timeProvider, profileResolver, "stream", "Stream primary resource to standard output.")
    {
    }

    public StreamCommand(
        IToolLogHandlerProvider toolLogHandlerProvider,
        IArtifactToolRegistryStore pluginStore,
        IToolPropertyProvider toolPropertyProvider,
        TimeProvider timeProvider,
        IProfileResolver profileResolver,
        string name,
        string? description = null)
        : base(toolLogHandlerProvider, pluginStore, toolPropertyProvider, name, description)
    {
        TimeProvider = timeProvider;
        ProfileResolver = profileResolver;
        ProfileFileArg = new Argument<string>("profile", "Profile file") { HelpName = "profile", Arity = ArgumentArity.ExactlyOne };
        AddArgument(ProfileFileArg);
    }

    protected override async Task<int> RunAsync(InvocationContext context, CancellationToken cancellationToken)
    {
        IToolLogHandler l = ToolLogHandlerProvider.GetStreamToolLogHandler();
        List<ArtifactToolProfile> profiles = new();
        ResolveAndAddProfiles(ProfileResolver, profiles, context.ParseResult.GetValueForArgument(ProfileFileArg));
        if (profiles.Count == 0)
        {
            throw new ArtUserException("No profiles were loaded from specified inputs, this command requires exactly one");
        }
        if (profiles.Count != 1)
        {
            throw new ArtUserException("Multiple profiles were loaded from specified inputs, this command requires exactly one");
        }
        var profile = PrepareProfile(context, profiles[0]);
        using var arm = new InMemoryArtifactRegistrationManager();
        using var adm = new InMemoryArtifactDataManager();
        (bool getArtifactRetrievalTimestamps, bool getResourceRetrievalTimestamps) = GetArtifactRetrievalOptions(context);
        using var tool = await GetToolAsync(profile, arm, adm, TimeProvider, getArtifactRetrievalTimestamps, getResourceRetrievalTimestamps, cancellationToken).ConfigureAwait(false);
        var listProxy = new ArtifactToolListProxy(tool, ArtifactToolListOptions.Default, l);
        var res = await listProxy.ListAsync(cancellationToken).ToListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        if (res.Count == 0)
        {
            l.Log($"No artifacts found for profile {profile}, this command requires exactly one", null, LogLevel.Error);
            return 77;
        }
        if (res.Count > 1)
        {
            l.Log($"Multiple artifacts found for profile {profile}, this command requires exactly one", null, LogLevel.Error);
            return 78;
        }
        var artifact = res[0];
        if (artifact.PrimaryResource is not { } primaryResource)
        {
            l.Log($"No primary resource available for artifact {artifact.Info.Key}, this command requires one", null, LogLevel.Error);
            return 79;
        }
        if (!primaryResource.CanExportStream)
        {
            l.Log($"Primary resource {primaryResource} does not support exporting, this command requires this functionality", null, LogLevel.Error);
        }
        var output = ToolLogHandlerProvider.GetOutStream();
        await using var output1 = output.ConfigureAwait(false);
        await primaryResource.ExportStreamAsync(output, useLogger: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        return 0;
    }
}
