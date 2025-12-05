using System.Diagnostics.CodeAnalysis;
using Art.Tesler.Profiles;
using Art.Tesler.Properties;
using Microsoft.Extensions.Time.Testing;

namespace Art.Tesler.Tests;

public class ChecksumTests : CommandTestBase
{
    private const string ProfileName = "profile";

    protected ArcCommand? Command;

    [MemberNotNull(nameof(Command))]
    protected void InitCommandDefault(
        IToolLogHandlerProvider toolLogHandlerProvider,
        IArtifactToolRegistryStore artifactToolRegistryStore,
        IToolPropertyProvider toolPropertyProvider,
        ITeslerDataProvider dataProvider,
        ITeslerRegistrationProvider registrationProvider,
        TimeProvider timeProvider,
        IProfileResolver profileResolver)
    {
        Command = new ArcCommand(toolLogHandlerProvider, artifactToolRegistryStore, toolPropertyProvider, dataProvider, registrationProvider, timeProvider, profileResolver);
    }

    [Test]
    public void NoChecksumPassed_Success()
    {
        var store = GetEmptyStore();
        var toolPropertyProvider = CreateInMemoryPropertyProvider();
        var dataProvider = CreateSharedMemoryDataProvider();
        var registrationProvider = CreateSharedMemoryRegistrationProvider();
        var profileResolver = CreateDictionaryProfileResolver(ProfileName);
        CreateOutputs(out var toolOutput, out var console);
        InitCommandDefault(toolOutput, store, toolPropertyProvider, dataProvider, registrationProvider, new FakeTimeProvider(), profileResolver);
        string[] line = { ProfileName };
        Assert.That(InvokeCommand(Command, line, console), Is.EqualTo(0));
        Assert.That(Out.ToString(), Is.Empty);
        Assert.That(Error.ToString(), Is.Empty);
    }

    [Test]
    public void KnownChecksumPassed_Success()
    {
        var store = GetEmptyStore();
        var toolPropertyProvider = CreateInMemoryPropertyProvider();
        var dataProvider = CreateSharedMemoryDataProvider();
        var registrationProvider = CreateSharedMemoryRegistrationProvider();
        var profileResolver = CreateDictionaryProfileResolver(ProfileName);
        CreateOutputs(out var toolOutput, out var console);
        InitCommandDefault(toolOutput, store, toolPropertyProvider, dataProvider, registrationProvider, new FakeTimeProvider(), profileResolver);
        string[] line = { ProfileName, "--hash", "SHA256" };
        Assert.That(InvokeCommand(Command, line, console), Is.EqualTo(0));
        Assert.That(Out.ToString(), Is.Empty);
        Assert.That(Error.ToString(), Is.Empty);
    }

    [Test]
    public void BadChecksumPassed_Fails()
    {
        var store = GetEmptyStore();
        var toolPropertyProvider = CreateInMemoryPropertyProvider();
        var dataProvider = CreateSharedMemoryDataProvider();
        var registrationProvider = CreateSharedMemoryRegistrationProvider();
        var profileResolver = CreateDictionaryProfileResolver(ProfileName);
        CreateOutputs(out var toolOutput, out var console);
        InitCommandDefault(toolOutput, store, toolPropertyProvider, dataProvider, registrationProvider, new FakeTimeProvider(), profileResolver);
        string[] line = { ProfileName, "--hash", "BAD_CHECKSUM" };
        Assert.That(InvokeCommand(Command, line, console), Is.Not.EqualTo(0));
        Assert.That(Out.ToString(), Is.Empty);
        Assert.That(Error.ToString(), Is.Not.Empty);
    }
}
