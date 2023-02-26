﻿using System.CommandLine;
using Art.Common;
using Art.Tesler.Cookie;
using Art.Tesler.Database;

namespace Art.Tesler;

public class TeslerRootCommand : RootCommand
{
    public static TeslerRootCommand Create<TPluginStore>(TPluginStore pluginStore) where TPluginStore : IArtifactToolRegistryStore
    {
        return new TeslerRootCommand<TPluginStore>(pluginStore)
        {
            new ArcCommand<TPluginStore>(pluginStore),
            new DumpCommand<TPluginStore>(pluginStore),
            new FindCommand<TPluginStore>(pluginStore),
            new ListCommand<TPluginStore>(pluginStore),
            new RehashCommand(),
            new ToolsCommand<TPluginStore>(pluginStore),
            new ValidateCommand<TPluginStore>(pluginStore),
            new DatabaseCommand(),
            new CookieCommand()
        };
    }

    public static TeslerRootCommand Create(IArtifactToolRegistry artifactToolRegistry)
    {
        return Create(new StaticArtifactToolRegistryStore(artifactToolRegistry));
    }
}

public class TeslerRootCommand<TPluginStore> : TeslerRootCommand where TPluginStore : IArtifactToolRegistryStore
{
    protected TPluginStore PluginStore;

    public TeslerRootCommand(TPluginStore pluginStore)
    {
        PluginStore = pluginStore;
    }
}