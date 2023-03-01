﻿using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using Art.Common;
using Art.Common.Management;
using Art.Common.Proxies;

namespace Art.Tesler;

public class ArcCommand : ToolCommandBase
{
    protected ITeslerDataProvider DataProvider;

    protected ITeslerRegistrationProvider RegistrationProvider;

    protected IProfileResolver ProfileResolver;

    protected Option<string> HashOption;

    protected Argument<List<string>> ProfileFilesArg;

    protected Option<ResourceUpdateMode> UpdateOption;

    protected Option<bool> FullOption;

    protected Option<ArtifactSkipMode> SkipOption;

    protected Option<bool> FastExitOption;

    protected Option<bool> NullOutputOption;

    public ArcCommand(
        IArtifactToolRegistryStore pluginStore,
        IDefaultPropertyProvider defaultPropertyProvider,
        IToolLogHandlerProvider toolLogHandlerProvider,
        ITeslerDataProvider dataProvider,
        ITeslerRegistrationProvider registrationProvider,
        IProfileResolver profileResolver)
        : this(pluginStore, defaultPropertyProvider, toolLogHandlerProvider, dataProvider, registrationProvider, profileResolver, "arc", "Execute archival artifact tools.")
    {
    }

    public ArcCommand(IArtifactToolRegistryStore pluginStore,
        IDefaultPropertyProvider defaultPropertyProvider,
        IToolLogHandlerProvider toolLogHandlerProvider,
        ITeslerDataProvider dataProvider,
        ITeslerRegistrationProvider registrationProvider,
        IProfileResolver profileResolver,
        string name,
        string? description = null)
        : base(pluginStore, defaultPropertyProvider, toolLogHandlerProvider, name, description)
    {
        DataProvider = dataProvider;
        DataProvider.Initialize(this);
        RegistrationProvider = registrationProvider;
        RegistrationProvider.Initialize(this);
        ProfileResolver = profileResolver;
        HashOption = new Option<string>(new[] { "-h", "--hash" }, $"Checksum algorithm ({Common.ChecksumAlgorithms})");
        HashOption.SetDefaultValue(Common.DefaultChecksumAlgorithm);
        AddOption(HashOption);
        ProfileFilesArg = new Argument<List<string>>("profile", "Profile file(s)") { HelpName = "profile", Arity = ArgumentArity.OneOrMore };
        AddArgument(ProfileFilesArg);
        UpdateOption = new Option<ResourceUpdateMode>(new[] { "-u", "--update" }, $"Resource update mode ({Common.ResourceUpdateModes})") { ArgumentHelpName = "mode" };
        UpdateOption.SetDefaultValue(ResourceUpdateMode.ArtifactHard);
        AddOption(UpdateOption);
        FullOption = new Option<bool>(new[] { "-f", "--full" }, "Only process full artifacts");
        AddOption(FullOption);
        SkipOption = new Option<ArtifactSkipMode>(new[] { "-s", "--skip" }, $"Skip artifacts ({Common.ArtifactSkipModes})");
        SkipOption.ArgumentHelpName = "mode";
        SkipOption.SetDefaultValue(ArtifactSkipMode.None);
        AddOption(SkipOption);
        FastExitOption = new Option<bool>(new[] { "-z", "--fast-exit" }, $"Equivalent to -s/--skip {nameof(ArtifactSkipMode.FastExit)}");
        AddOption(FastExitOption);
        NullOutputOption = new Option<bool>(new[] { "--null-output" }, "Send resources to the void");
        AddOption(NullOutputOption);
    }

    protected override async Task<int> RunAsync(InvocationContext context)
    {
        ChecksumSource? checksumSource;
        string? hash = context.ParseResult.HasOption(HashOption) ? context.ParseResult.GetValueForOption(HashOption) : null;
        hash = string.Equals(hash, "none", StringComparison.InvariantCultureIgnoreCase) ? null : hash;
        if (hash == null)
        {
            checksumSource = null;
        }
        else
        {
            if (!ChecksumSource.DefaultSources.TryGetValue(hash, out checksumSource))
            {
                PrintErrorMessage(Common.GetInvalidHashMessage(hash), context.Console);
                return 2;
            }
        }
        ResourceUpdateMode update = context.ParseResult.GetValueForOption(UpdateOption);
        bool full = context.ParseResult.GetValueForOption(FullOption);
        ArtifactSkipMode skip = context.ParseResult.GetValueForOption(SkipOption);
        bool fastExit = context.ParseResult.GetValueForOption(FastExitOption);
        bool nullOutput = context.ParseResult.GetValueForOption(NullOutputOption);
        ArtifactToolDumpOptions options = new(update, !full, fastExit ? ArtifactSkipMode.FastExit : skip, checksumSource);
        using var adm = nullOutput ? new NullArtifactDataManager() : DataProvider.CreateArtifactDataManager(context);
        using var arm = RegistrationProvider.CreateArtifactRegistrationManager(context);
        IToolLogHandler l = ToolLogHandlerProvider.GetDefaultToolLogHandler(context.Console);
        List<ArtifactToolProfile> profiles = new();
        foreach (string profileFile in context.ParseResult.GetValueForArgument(ProfileFilesArg))
        {
            ResolveAndAddProfiles(ProfileResolver, profiles, profileFile);
        }
        string? cookieFile = context.ParseResult.HasOption(CookieFileOption) ? context.ParseResult.GetValueForOption(CookieFileOption) : null;
        string? userAgent = context.ParseResult.HasOption(UserAgentOption) ? context.ParseResult.GetValueForOption(UserAgentOption) : null;
        IEnumerable<string> properties = context.ParseResult.HasOption(PropertiesOption) ? context.ParseResult.GetValueForOption(PropertiesOption)! : Array.Empty<string>();
        profiles = profiles.Select(p => p.GetWithConsoleOptions(DefaultPropertyProvider, properties, cookieFile, userAgent, context.Console)).ToList();
        foreach (ArtifactToolProfile profile in profiles)
        {
            var plugin = PluginStore.LoadRegistry(ArtifactToolProfileUtil.GetID(profile.Tool));
            await ArtifactDumping.DumpAsync(plugin, profile, arm, adm, options, l).ConfigureAwait(false);
        }
        return 0;
    }
}
