using System.Diagnostics.CodeAnalysis;
using Art.Common;
using Art.Common.Logging;
using Art.Tesler.Properties;
using Art.TestsBase;
using Microsoft.Extensions.Time.Testing;

namespace Art.Tesler.Tests;

public class LoggingTests : CommandTestBase
{
    protected DumpCommand? Command;

    private const string Message = "message_here";
    private const string Group = "group_here";
    private const string OutputDelimiter = "🥔";

    [MemberNotNull(nameof(Command))]
    protected void InitCommandDefault(
        IToolLogHandlerProvider toolLogHandlerProvider,
        IArtifactToolRegistryStore artifactToolRegistryStore,
        IToolPropertyProvider toolPropertyProvider,
        ITeslerDataProvider dataProvider,
        ITeslerRegistrationProvider registrationProvider,
        TimeProvider timeProvider)
    {
        Command = new DumpCommand(toolLogHandlerProvider, artifactToolRegistryStore, toolPropertyProvider, dataProvider, registrationProvider, timeProvider);
    }

    [Test]
    public void Dump_LogInfoTool_OutputMatches()
    {
        CreateOutputs(out var toolOutput, out var console, OutputDelimiter);
        string toolString = ArtifactToolIDUtil.CreateToolString<ProgrammableArtifactDumpTool>();
        int code = Execute(toolOutput, console, t => t.LogInformation(Message), new[] { "-t", toolString, "-g", Group });
        Assert.That(code, Is.EqualTo(0));
        string outputContent = Out.ToString();
        Assert.That(outputContent, Is.Not.Empty);
        Assert.That(outputContent, Is.EqualTo(ConstructOutput(OutputDelimiter, toolString, Group, Message, null, LogLevel.Information)));
        Assert.That(Error.ToString(), Is.Empty);
    }

    [Test]
    public void Dump_LogTitleTool_OutputMatches()
    {
        CreateOutputs(out var toolOutput, out var console, OutputDelimiter);
        string toolString = ArtifactToolIDUtil.CreateToolString<ProgrammableArtifactDumpTool>();
        int code = Execute(toolOutput, console, t => t.LogTitle(Message), new[] { "-t", toolString, "-g", Group });
        Assert.That(code, Is.EqualTo(0));
        string outputContent = Out.ToString();
        Assert.That(outputContent, Is.Not.Empty);
        Assert.That(outputContent, Is.EqualTo(ConstructOutput(OutputDelimiter, toolString, Group, Message, null, LogLevel.Title)));
        Assert.That(Error.ToString(), Is.Empty);
    }

    [Test]
    public void Dump_LogEntryTool_OutputMatches()
    {
        CreateOutputs(out var toolOutput, out var console, OutputDelimiter);
        string toolString = ArtifactToolIDUtil.CreateToolString<ProgrammableArtifactDumpTool>();
        int code = Execute(toolOutput, console, t => t.LogEntry(Message), new[] { "-t", toolString, "-g", Group });
        Assert.That(code, Is.EqualTo(0));
        string outputContent = Out.ToString();
        Assert.That(outputContent, Is.Not.Empty);
        Assert.That(outputContent, Is.EqualTo(ConstructOutput(OutputDelimiter, toolString, Group, Message, null, LogLevel.Entry)));
        Assert.That(Error.ToString(), Is.Empty);
    }

    [Test]
    public void Dump_LogWarningTool_OutputMatches()
    {
        CreateOutputs(out var toolOutput, out var console, OutputDelimiter);
        string toolString = ArtifactToolIDUtil.CreateToolString<ProgrammableArtifactDumpTool>();
        int code = Execute(toolOutput, console, t => t.LogWarning(Message), new[] { "-t", toolString, "-g", Group });
        Assert.That(code, Is.EqualTo(0));
        Assert.That(Out.ToString(), Is.Empty);
        string warnContent = Warn.ToString();
        Assert.That(warnContent, Is.Not.Empty);
        Assert.That(warnContent, Is.EqualTo(ConstructWarnOutput(OutputDelimiter, toolString, Group, Message, null, LogLevel.Warning)));
    }

    [Test]
    public void Dump_LogErrorTool_OutputMatches()
    {
        CreateOutputs(out var toolOutput, out var console, OutputDelimiter);
        string toolString = ArtifactToolIDUtil.CreateToolString<ProgrammableArtifactDumpTool>();
        int code = Execute(toolOutput, console, t => t.LogError(Message), new[] { "-t", toolString, "-g", Group });
        Assert.That(code, Is.EqualTo(0));
        Assert.That(Out.ToString(), Is.Empty);
        string errorContent = Error.ToString();
        Assert.That(errorContent, Is.Not.Empty);
        Assert.That(errorContent, Is.EqualTo(ConstructErrorOutput(OutputDelimiter, toolString, Group, Message, null, LogLevel.Error)));
    }

    private int Execute(IToolLogHandlerProvider toolLogHandlerProvider,TestConsole testConsole, Action<ProgrammableArtifactDumpTool> action, string[] line)
    {
        var store = GetSingleStore(ProgrammableArtifactDumpTool.CreateRegistryEntry(t => action(t)));
        var toolPropertyProvider = CreateInMemoryPropertyProvider();
        var dataProvider = CreateSharedMemoryDataProvider();
        var registrationProvider = CreateSharedMemoryRegistrationProvider();
        InitCommandDefault(toolLogHandlerProvider, store, toolPropertyProvider, dataProvider, registrationProvider, new FakeTimeProvider());
        return InvokeCommand(Command, line, testConsole);
    }

    private static string ConstructOutput(string outputDelimiter, string toolString, string group, string? title, string? body, LogLevel logLevel)
    {
        var expectedOutput = new StringWriter { NewLine = outputDelimiter };
        var expectedOutputHandler = new PlainLogHandler(expectedOutput, TextWriter.Null, TextWriter.Null, false);
        expectedOutputHandler.Log(toolString, group, title, body, logLevel);
        return expectedOutput.ToString();
    }

    private static string ConstructWarnOutput(string outputDelimiter, string toolString, string group, string? title, string? body, LogLevel logLevel)
    {
        var expectedWarnOutput = new StringWriter { NewLine = outputDelimiter };
        var expectedWarnOutputHandler = new PlainLogHandler(TextWriter.Null, expectedWarnOutput, TextWriter.Null, false);
        expectedWarnOutputHandler.Log(toolString, group, title, body, logLevel);
        return expectedWarnOutput.ToString();
    }

    private static string ConstructErrorOutput(string outputDelimiter, string toolString, string group, string? title, string? body, LogLevel logLevel)
    {
        var expectedErrorOutput = new StringWriter { NewLine = outputDelimiter };
        var expectedErrorOutputHandler = new PlainLogHandler(TextWriter.Null, TextWriter.Null, expectedErrorOutput, false);
        expectedErrorOutputHandler.Log(toolString, group, title, body, logLevel);
        return expectedErrorOutput.ToString();
    }
}
