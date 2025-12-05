using System.Diagnostics.CodeAnalysis;
using Art.Common;
using Art.Tesler.Properties;
using Art.TestsBase;
using Microsoft.Extensions.Time.Testing;

namespace Art.Tesler.Tests;

public class DumpCommandTests : CommandTestBase
{
    protected DumpCommand? Command;

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
    public void EmptyInvocation_Fails()
    {
        var store = GetSingleStore(ProgrammableArtifactDumpTool.CreateRegistryEntry(_ => { }));
        var toolPropertyProvider = CreateInMemoryPropertyProvider();
        var dataProvider = CreateSharedMemoryDataProvider();
        var registrationProvider = CreateSharedMemoryRegistrationProvider();
        CreateOutputs(out var toolOutput, out var console);
        InitCommandDefault(toolOutput, store, toolPropertyProvider, dataProvider, registrationProvider, new FakeTimeProvider());
        Assert.That(InvokeCommand(Command, Array.Empty<string>(), console), Is.Not.EqualTo(0));
        Assert.That(Out.ToString(), Is.Not.Empty);
        Assert.That(Error.ToString(), Is.Not.Empty);
    }

    [Test]
    public void MissingTool_Fails()
    {
        var store = GetSingleStore(ProgrammableArtifactDumpTool.CreateRegistryEntry(_ => { }));
        var toolPropertyProvider = CreateInMemoryPropertyProvider();
        var dataProvider = CreateSharedMemoryDataProvider();
        var registrationProvider = CreateSharedMemoryRegistrationProvider();
        CreateOutputs(out var toolOutput, out var console);
        InitCommandDefault(toolOutput, store, toolPropertyProvider, dataProvider, registrationProvider, new FakeTimeProvider());
        string[] line = { "-t", new ArtifactToolID("NOT_AN_ASSEMBLY", "MALO").GetToolString() };
        Assert.That(InvokeCommand(Command, line, console), Is.Not.EqualTo(0));
        Assert.That(Out.ToString(), Is.Empty);
        Assert.That(Error.ToString(), Is.Not.Empty);
    }

    [Test]
    public void NoopTool_Success()
    {
        var store = GetSingleStore(ProgrammableArtifactDumpTool.CreateRegistryEntry(_ => { }));
        var toolPropertyProvider = CreateInMemoryPropertyProvider();
        var dataProvider = CreateSharedMemoryDataProvider();
        var registrationProvider = CreateSharedMemoryRegistrationProvider();
        CreateOutputs(out var toolOutput, out var console);
        InitCommandDefault(toolOutput, store, toolPropertyProvider, dataProvider, registrationProvider, new FakeTimeProvider());
        string[] line = { "-t", ArtifactToolIDUtil.CreateToolString<ProgrammableArtifactDumpTool>() };
        Assert.That(InvokeCommand(Command, line, console), Is.EqualTo(0));
        Assert.That(Out.ToString(), Is.Empty);
        Assert.That(Error.ToString(), Is.Empty);
    }
}
