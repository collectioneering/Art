using System.Diagnostics.CodeAnalysis;
using Art.Common;
using Art.Common.Resources;
using Art.Tesler.Properties;
using Art.TestsBase;
using Microsoft.Extensions.Time.Testing;

namespace Art.Tesler.Tests;

public class FindCommandTests : CommandTestBase
{
    protected FindCommand? Command;

    [MemberNotNull(nameof(Command))]
    protected void InitCommandDefault(
        IToolLogHandlerProvider toolLogHandlerProvider,
        IArtifactToolRegistryStore artifactToolRegistryStore,
        IToolPropertyProvider toolPropertyProvider,
        TimeProvider timeProvider)
    {
        Command = new FindCommand(toolLogHandlerProvider, artifactToolRegistryStore, toolPropertyProvider, timeProvider);
    }

    [Test]
    public void EmptyInvocation_Fails()
    {
        var store = GetSingleStore(ProgrammableArtifactFindTool.CreateRegistryEntry((_, _) => null));
        var toolPropertyProvider = CreateInMemoryPropertyProvider();
        CreateObjectOutputs(out var toolOutput, out var console);
        InitCommandDefault(toolOutput, store, toolPropertyProvider, new FakeTimeProvider());
        int rc = InvokeCommand(Command, Array.Empty<string>(), console);
        Assert.That(Out.ToString(), Is.Not.Empty);
        Assert.That(OutQueue, Is.Empty);
        Assert.That(Error.ToString(), Is.Not.Empty);
        Assert.That(ErrorQueue, Is.Empty);
        Assert.That(rc, Is.Not.EqualTo(0));
    }

    [Test]
    public void MissingTool_Fails()
    {
        var store = GetSingleStore(ProgrammableArtifactFindTool.CreateRegistryEntry((_, _) => null));
        var toolPropertyProvider = CreateInMemoryPropertyProvider();
        CreateObjectOutputs(out var toolOutput, out var console);
        InitCommandDefault(toolOutput, store, toolPropertyProvider, new FakeTimeProvider());
        string[] line = { "-t", new ArtifactToolID("NOT_AN_ASSEMBLY", "MALO").GetToolString() };
        int rc = InvokeCommand(Command, line, console);
        Assert.That(Out.ToString(), Is.Not.Empty);
        Assert.That(OutQueue, Is.Empty);
        Assert.That(Error.ToString(), Is.Not.Empty);
        Assert.That(ErrorQueue, Is.Empty);
        Assert.That(rc, Is.Not.EqualTo(0));
    }

    [Test]
    public void MissingArgId_Success()
    {
        var store = GetSingleStore(ProgrammableArtifactFindTool.CreateRegistryEntry((_, _) => null));
        var toolPropertyProvider = CreateInMemoryPropertyProvider();
        CreateObjectOutputs(out var toolOutput, out var console);
        InitCommandDefault(toolOutput, store, toolPropertyProvider, new FakeTimeProvider());
        string[] line = { "-t", ArtifactToolIDUtil.CreateToolString<ProgrammableArtifactFindTool>() };
        int rc = InvokeCommand(Command, line, console);
        Assert.That(Out.ToString(), Is.Not.Empty);
        Assert.That(OutQueue, Is.Empty);
        Assert.That(Error.ToString(), Is.Not.Empty);
        Assert.That(ErrorQueue, Is.Empty);
        Assert.That(rc, Is.Not.EqualTo(0));
    }

    [Test]
    public void NoResult_Success()
    {
        const string search = "ID_1";
        var store = GetSingleStore(ProgrammableArtifactFindTool.CreateRegistryEntry((_, _) => null));
        var toolPropertyProvider = CreateInMemoryPropertyProvider();
        CreateObjectOutputs(out var toolOutput, out var console);
        InitCommandDefault(toolOutput, store, toolPropertyProvider, new FakeTimeProvider());
        string[] line = { "-t", ArtifactToolIDUtil.CreateToolString<ProgrammableArtifactFindTool>(), search };
        int rc = InvokeCommand(Command, line, console);
        Assert.That(Out.ToString(), Is.Empty);
        Assert.That(OutQueue, Is.Empty);
        Assert.That(Error.ToString(), Is.Not.Empty);
        Assert.That(ErrorQueue, Is.Empty);
        Assert.That(rc, Is.EqualTo(0));
    }

    [Test]
    public void Result_Success()
    {
        const string group = "GROUP_1";
        const string search = "ID_1";
        var store = GetSingleStore(ProgrammableArtifactFindTool.CreateRegistryEntry((v, k) =>
        {
            if (search.Equals(k))
            {
                var data = v.CreateData(k);
                data.String("RES_1_CONTENT", "RES_1").Commit();
                return data;
            }
            return null;
        }));
        var toolPropertyProvider = CreateInMemoryPropertyProvider();
        CreateObjectOutputs(out var toolOutput, out var console);
        InitCommandDefault(toolOutput, store, toolPropertyProvider, new FakeTimeProvider());
        string toolString = ArtifactToolIDUtil.CreateToolString<ProgrammableArtifactFindTool>();
        string[] line = { "-t", toolString, "-g", group, search };
        int rc = InvokeCommand(Command, line, console);
        Assert.That(Out.ToString(), Is.Empty);
        Assert.That(OutQueue, Has.Count.EqualTo(1));
        Assert.That(Error.ToString(), Is.Empty);
        Assert.That(ErrorQueue, Is.Empty);
        Assert.That(rc, Is.EqualTo(0));
        var vq = OutQueue.Dequeue();
        Assert.That(vq, Is.InstanceOf<ArtifactDataObjectLog>());
        var data = ((ArtifactDataObjectLog)vq).ArtifactData;
        var key = data.Info.Key;
        Assert.That(key.Id, Is.EqualTo(search));
        Assert.That(key.Tool, Is.EqualTo(toolString));
        Assert.That(key.Group, Is.EqualTo(group));
        var rkey1 = new ArtifactResourceKey(key, "RES_1");
        Assert.That(data.Keys, Is.EquivalentTo(new[] { rkey1 }));
        Assert.That(data[rkey1], Is.InstanceOf<StringArtifactResourceInfo>().With.Property("Resource").EqualTo("RES_1_CONTENT"));
    }
}
