using Art.Common.Management;
using Art.DbTestsBase;

namespace Art.Common.Tests;

public class InMemoryArtifactRegistrationManager_Tests : ArtifactRegistrationManagerTestsBase
{
    [Fact]
    public async Task ReadWrite_Success()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        using IArtifactRegistrationManager r = new InMemoryArtifactRegistrationManager();
        await TestReadWriteDatabase(r, testCancellationToken);
    }

    [Fact]
    public async Task ReadWrite_MultiAccess_Success()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        using InMemoryArtifactRegistrationManager r = new(isReadOnly: false);
        await TestReadWriteDatabase(r, testCancellationToken);
        {
            using IArtifactRegistrationManager r2 = new InMemoryArtifactRegistrationManager(r, isReadOnly: true);
            await VerifyWrittenDatabase(r2, testCancellationToken);
        }
    }

    [Fact]
    public async Task ReadOnly_PreventsWrite()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        using InMemoryArtifactRegistrationManager r = new(isReadOnly: false);
        await TestReadWriteDatabase(r, testCancellationToken);
        {
            using IArtifactRegistrationManager r2 = new InMemoryArtifactRegistrationManager(r, isReadOnly: true);
            await TestDatabaseReadOnly(r2, false, testCancellationToken);
        }
    }
}
