using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text;
using System.Text.RegularExpressions;

namespace Art.Tesler;

public class ToolsCommand : CommandBase
{
    protected IArtifactToolRegistryStore PluginStore;

    protected Option<string> SearchOption;

    protected Option<bool> DetailedOption;

    public ToolsCommand(
        IOutputControl toolOutput,
        IArtifactToolRegistryStore pluginStore)
        : this(toolOutput, pluginStore, "tools", "List available tools.")
    {
    }

    public ToolsCommand(
        IOutputControl toolOutput,
        IArtifactToolRegistryStore pluginStore,
        string name,
        string? description = null)
        : base(toolOutput, name, description)
    {
        PluginStore = pluginStore;
        SearchOption = new Option<string>("-s", "--search") { HelpName = "pattern", Description = "Search pattern" };
        Add(SearchOption);
        DetailedOption = new Option<bool>("--detailed") { Description = "Show detailed information on entries" };
        Add(DetailedOption);
    }

    protected override Task<int> RunAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        foreach (var plugin in PluginStore.LoadAllRegistries())
        {
            string? search = parseResult.GetValue(SearchOption);
            Regex? re = search != null ? Common.GetFilterRegex(search, false, false) : null;
            foreach (var desc in plugin.GetToolDescriptions()
                         .Where(v => re?.IsMatch(v.Id.GetToolString()) ?? true))
            {
                Common.PrintFormat(desc.Id.GetToolString(), parseResult.GetValue(DetailedOption), () =>
                {
                    bool canFind = desc.Type.IsAssignableTo(typeof(IArtifactFindTool));
                    bool canList = desc.Type.IsAssignableTo(typeof(IArtifactListTool));
                    bool canDump = canList || desc.Type.IsAssignableTo(typeof(IArtifactDumpTool));
                    bool canSelect = desc.Type.IsAssignableTo(typeof(IArtifactToolSelector<string>));
                    IEnumerable<string> capabilities = Enumerable.Empty<string>();
                    if (canFind) capabilities = capabilities.Append("find");
                    if (canList) capabilities = capabilities.Append("list");
                    if (canDump) capabilities = capabilities.Append("arc");
                    if (canSelect) capabilities = capabilities.Append("select");
                    capabilities = capabilities.DefaultIfEmpty("none");
                    return new StringBuilder("Capabilities: ").AppendJoin(", ", capabilities).ToString();
                }, ToolOutput);
            }
        }
        return Task.FromResult(0);
    }
}
