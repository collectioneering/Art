using System.Diagnostics.CodeAnalysis;
using Art.TestsBase;

namespace Art.Tesler.Tests;

public class ToolsCommandTests : CommandTestBase
{
    protected ToolsCommand? Command;

    [MemberNotNull(nameof(Command))]
    protected void InitCommandDefault(IOutputControl toolOutput, IArtifactToolRegistryStore artifactToolRegistryStore)
    {
        Command = new ToolsCommand(toolOutput, artifactToolRegistryStore);
    }

    [Test]
    public void DefaultExecution_Empty_Success()
    {
        CreateOutputs(out var toolOutput, out var console);
        InitCommandDefault(toolOutput, GetEmptyStore());
        Assert.That(InvokeCommand(Command, Array.Empty<string>(), console), Is.EqualTo(0));
        Assert.That(Out.ToString(), Is.Empty);
        Assert.That(Error.ToString(), Is.Empty);
    }

    [Test]
    public void DefaultExecution_Single_Success()
    {
        CreateOutputs(out var toolOutput, out var console);
        InitCommandDefault(toolOutput, GetSingleStore(ProgrammableArtifactDumpTool.CreateRegistryEntry(_ => { })));
        Assert.That(InvokeCommand(Command, Array.Empty<string>(), console), Is.EqualTo(0));
        Assert.That(Out.ToString(), Contains.Substring(nameof(ProgrammableArtifactDumpTool)));
        Assert.That(Error.ToString(), Is.Empty);
    }

    [Test]
    public void Search_NoMatch_NotFound()
    {
        CreateOutputs(out var toolOutput, out var console);
        InitCommandDefault(toolOutput, GetSingleStore(ProgrammableArtifactDumpTool.CreateRegistryEntry(_ => { })));
        string[] line = { "-s", "$$NOT_A_REAL_TOOL$$" };
        Assert.That(InvokeCommand(Command, line, console), Is.EqualTo(0));
        Assert.That(Out.ToString(), Is.Empty);
        Assert.That(Error.ToString(), Is.Empty);
    }

    [Test]
    public void Search_SingleMatching_Found()
    {
        CreateOutputs(out var toolOutput, out var console);
        InitCommandDefault(toolOutput, GetSingleStore(ProgrammableArtifactDumpTool.CreateRegistryEntry(_ => { })));
        string[] line = { "-s", nameof(ProgrammableArtifactDumpTool) };
        Assert.That(InvokeCommand(Command, line, console), Is.EqualTo(0));
        Assert.That(Out.ToString(), Contains.Substring(nameof(ProgrammableArtifactDumpTool)));
        Assert.That(Error.ToString(), Is.Empty);
    }
}
