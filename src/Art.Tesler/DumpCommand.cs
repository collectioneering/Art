﻿using System.CommandLine;
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
        HashOption = new Option<string>(new[] { "-h", "--hash" }, $"Checksum algorithm ({Common.ChecksumAlgorithms})");
        HashOption.SetDefaultValue(Common.DefaultChecksumAlgorithm);
        AddOption(HashOption);
        NoDatabaseOption = new Option<bool>(new[] { "--no-database" }, "Don't use database to track resources");
        AddOption(NoDatabaseOption);
        ProfileFileOption = new Option<string>(new[] { "-i", "--input" }, "Profile file") { ArgumentHelpName = "file" };
        AddOption(ProfileFileOption);
        ToolOption = new Option<string>(new[] { "-t", "--tool" }, "Tool to use or filter profiles by") { ArgumentHelpName = "name" };
        AddOption(ToolOption);
        GroupOption = new Option<string>(new[] { "-g", "--group" }, "Group to use or filter profiles by") { ArgumentHelpName = "name" };
        AddOption(GroupOption);
        AddValidator(v =>
        {
            if (v.GetValueForOption(ProfileFileOption) == null && v.GetValueForOption(ToolOption) == null)
            {
                v.ErrorMessage = $"At least one of {ProfileFileOption.Aliases.First()} or {ToolOption.Aliases.First()} must be passed.";
            }
        });
    }

    protected override async Task<int> RunAsync(InvocationContext context, CancellationToken cancellationToken)
    {
        using var adm = DataProvider.CreateArtifactDataManager(context);
        if (context.ParseResult.GetValueForOption(NoDatabaseOption))
        {
            InMemoryArtifactRegistrationManager arm = new();
            return await RunAsync(context, adm, arm, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            using var arm = RegistrationProvider.CreateArtifactRegistrationManager(context);
            return await RunAsync(context, adm, arm, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task<int> RunAsync(InvocationContext context, IArtifactDataManager adm, IArtifactRegistrationManager arm, CancellationToken cancellationToken)
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
                PrintErrorMessage(Common.GetInvalidHashMessage(hash), ToolOutput);
                return 2;
            }
        }
        string? profileFile = context.ParseResult.HasOption(ProfileFileOption) ? context.ParseResult.GetValueForOption(ProfileFileOption) : null;
        string? tool = context.ParseResult.HasOption(ToolOption) ? context.ParseResult.GetValueForOption(ToolOption) : null;
        string? group = context.ParseResult.HasOption(GroupOption) ? context.ParseResult.GetValueForOption(GroupOption) : null;
        (bool getArtifactRetrievalTimestamps, bool getResourceRetrievalTimestamps) = GetArtifactRetrievalOptions(context);
        if (profileFile == null)
        {
            return await ExecAsync(context, new ArtifactToolProfile(tool!, group, null), arm, adm, checksumSource, getArtifactRetrievalTimestamps, getResourceRetrievalTimestamps, cancellationToken).ConfigureAwait(false);
        }
        int ec = 0;
        foreach (ArtifactToolProfile profile in ArtifactToolProfileUtil.DeserializeProfilesFromFile(profileFile))
        {
            if (group != null && group != profile.Group || tool != null && tool != profile.Tool) continue;
            ec = Common.AccumulateErrorCode(await ExecAsync(context, profile, arm, adm, checksumSource, getArtifactRetrievalTimestamps, getResourceRetrievalTimestamps, cancellationToken).ConfigureAwait(false), ec);
        }
        return ec;
    }

    private async Task<int> ExecAsync(
        InvocationContext context,
        ArtifactToolProfile profile,
        IArtifactRegistrationManager arm,
        IArtifactDataManager adm,
        ChecksumSource? checksumSource,
        bool getArtifactRetrievalTimestamps,
        bool getResourceRetrievalTimestamps,
        CancellationToken cancellationToken)
    {
        ArtifactToolDumpOptions options = new(ChecksumSource: checksumSource);
        profile = PrepareProfile(context, profile);
        using var tool = await GetToolAsync(profile, arm, adm, TimeProvider, getArtifactRetrievalTimestamps, getResourceRetrievalTimestamps, cancellationToken).ConfigureAwait(false);
        ArtifactToolDumpProxy dProxy = new(tool, options, ToolLogHandlerProvider.GetDefaultToolLogHandler());
        await dProxy.DumpAsync(cancellationToken).ConfigureAwait(false);
        return 0;
    }
}
