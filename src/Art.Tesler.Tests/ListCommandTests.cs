using System.Diagnostics.CodeAnalysis;
using Art.Common;
using Art.Common.Resources;
using Art.Tesler.Properties;
using Art.TestsBase;
using Microsoft.Extensions.Time.Testing;

namespace Art.Tesler.Tests;

public class ListCommandTests : CommandTestBase
{
    protected ListCommand? Command;

    [MemberNotNull(nameof(Command))]
    protected void InitCommandDefault(
        IToolLogHandlerProvider toolLogHandlerProvider,
        IArtifactToolRegistryStore artifactToolRegistryStore,
        IToolPropertyProvider toolPropertyProvider,
        TimeProvider timeProvider)
    {
        Command = new ListCommand(toolLogHandlerProvider, artifactToolRegistryStore, toolPropertyProvider, timeProvider);
    }

    [Test]
    public void EmptyInvocation_Fails()
    {
        var store = GetSingleStore(ProgrammableArtifactListTool.CreateRegistryEntry(_ => []));
        var toolPropertyProvider = CreateInMemoryPropertyProvider();
        CreateObjectOutputs(out var toolOutput, out var console);
        InitCommandDefault(toolOutput, store, toolPropertyProvider, new FakeTimeProvider());
        int rc = InvokeCommand(Command, [], console);
        Assert.That(Out.ToString(), Is.Not.Empty);
        Assert.That(OutQueue, Is.Empty);
        Assert.That(Error.ToString(), Is.Not.Empty);
        Assert.That(ErrorQueue, Is.Empty);
        Assert.That(rc, Is.Not.EqualTo(0));
    }

    [Test]
    public void MissingTool_Fails()
    {
        var store = GetSingleStore(ProgrammableArtifactListTool.CreateRegistryEntry(_ => []));
        var toolPropertyProvider = CreateInMemoryPropertyProvider();
        CreateObjectOutputs(out var toolOutput, out var console);
        InitCommandDefault(toolOutput, store, toolPropertyProvider, new FakeTimeProvider());
        string[] line = ["-t", new ArtifactToolID("NOT_AN_ASSEMBLY", "MALO").GetToolString()];
        int rc = InvokeCommand(Command, line, console);
        Assert.That(Out.ToString(), Is.Empty);
        Assert.That(OutQueue, Is.Empty);
        Assert.That(Error.ToString(), Is.Not.Empty);
        Assert.That(ErrorQueue, Is.Empty);
        Assert.That(rc, Is.Not.EqualTo(0));
    }

    [Test]
    public void NoResults_Success()
    {
        var store = GetSingleStore(ProgrammableArtifactListTool.CreateRegistryEntry(_ => []));
        var toolPropertyProvider = CreateInMemoryPropertyProvider();
        CreateObjectOutputs(out var toolOutput, out var console);
        InitCommandDefault(toolOutput, store, toolPropertyProvider, new FakeTimeProvider());
        string[] line = ["-t", ArtifactToolIDUtil.CreateToolString<ProgrammableArtifactListTool>()];
        int rc = InvokeCommand(Command, line, console);
        Assert.That(Out.ToString(), Is.Empty);
        Assert.That(OutQueue, Is.Empty);
        Assert.That(Error.ToString(), Is.Empty);
        Assert.That(ErrorQueue, Is.Empty);
        Assert.That(rc, Is.EqualTo(0));
    }

    [Test]
    public void OneResult_Success()
    {
        const string group = "GROUP_1";
        var store = GetSingleStore(ProgrammableArtifactListTool.CreateRegistryEntry(v =>
        {
            var results = new List<IArtifactData>();
            var data = v.CreateData("ID_1");
            data.String("RES_1_CONTENT", "RES_1").Commit();
            results.Add(data);
            return results;
        }));
        var toolPropertyProvider = CreateInMemoryPropertyProvider();
        CreateObjectOutputs(out var toolOutput, out var console);
        InitCommandDefault(toolOutput, store, toolPropertyProvider, new FakeTimeProvider());
        string toolString = ArtifactToolIDUtil.CreateToolString<ProgrammableArtifactListTool>();
        string[] line = ["-t", toolString, "-g", group];
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
        Assert.That(key.Id, Is.EqualTo("ID_1"));
        Assert.That(key.Tool, Is.EqualTo(toolString));
        Assert.That(key.Group, Is.EqualTo(group));
        var rkey1 = new ArtifactResourceKey(key, "RES_1");
        Assert.That(data.Keys, Is.EquivalentTo([rkey1]));
        Assert.That(data[rkey1], Is.InstanceOf<StringArtifactResourceInfo>().With.Property("Resource").EqualTo("RES_1_CONTENT"));
    }

    [Test]
    public void MultiResult_Success()
    {
        const string group = "GROUP_1";
        var store = GetSingleStore(ProgrammableArtifactListTool.CreateRegistryEntry(v =>
        {
            var results = new List<IArtifactData>();
            var data1 = v.CreateData("ID_1");
            data1.String("RES_1_CONTENT", "RES_1").Commit();
            results.Add(data1);
            var data2 = v.CreateData("ID_2");
            data2.String("RES_2_CONTENT", "RES_2").Commit();
            results.Add(data2);
            return results;
        }));
        var toolPropertyProvider = CreateInMemoryPropertyProvider();
        CreateObjectOutputs(out var toolOutput, out var console);
        InitCommandDefault(toolOutput, store, toolPropertyProvider, new FakeTimeProvider());
        string toolString = ArtifactToolIDUtil.CreateToolString<ProgrammableArtifactListTool>();
        string[] line = ["-t", toolString, "-g", group];
        int rc = InvokeCommand(Command, line, console);
        Assert.That(Out.ToString(), Is.Empty);
        Assert.That(OutQueue, Has.Count.EqualTo(2));
        Assert.That(Error.ToString(), Is.Empty);
        Assert.That(ErrorQueue, Is.Empty);
        Assert.That(rc, Is.EqualTo(0));
        var vq1 = OutQueue.Dequeue();
        Assert.That(vq1, Is.InstanceOf<ArtifactDataObjectLog>());
        var data1 = ((ArtifactDataObjectLog)vq1).ArtifactData;
        var vq2 = OutQueue.Dequeue();
        Assert.That(vq2, Is.InstanceOf<ArtifactDataObjectLog>());
        var data2 = ((ArtifactDataObjectLog)vq2).ArtifactData;
        var key1 = data1.Info.Key;
        Assert.That(key1.Id, Is.EqualTo("ID_1"));
        Assert.That(key1.Tool, Is.EqualTo(toolString));
        Assert.That(key1.Group, Is.EqualTo(group));
        var rkey1 = new ArtifactResourceKey(key1, "RES_1");
        Assert.That(data1.Keys, Is.EquivalentTo([rkey1]));
        Assert.That(data1[rkey1], Is.InstanceOf<StringArtifactResourceInfo>().With.Property(nameof(StringArtifactResourceInfo.Resource)).EqualTo("RES_1_CONTENT"));
        var key2 = data2.Info.Key;
        Assert.That(key2.Id, Is.EqualTo("ID_2"));
        Assert.That(key2.Tool, Is.EqualTo(toolString));
        Assert.That(key2.Group, Is.EqualTo(group));
        var rkey2 = new ArtifactResourceKey(key2, "RES_2");
        Assert.That(data2.Keys, Is.EquivalentTo([rkey2]));
        Assert.That(data2[rkey2], Is.InstanceOf<StringArtifactResourceInfo>().With.Property(nameof(StringArtifactResourceInfo.Resource)).EqualTo("RES_2_CONTENT"));
    }
}
