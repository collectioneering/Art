using Art.Common.IO;
using Art.Common.Management;
using Art.DbTestsBase;

namespace Art.Common.Tests;

public class DiskArtifactRegistrationManager_Tests : ArtifactRegistrationManagerTestsBase
{
    public static string CreateDbDir()
    {
        string tmpParentDirectory = Path.Join(Path.GetTempPath(), "collectioneering_art_test_diskreg");
        Directory.CreateDirectory(tmpParentDirectory);
        string tempDirectory = ArtIOUtility.CreateRandomPath(tmpParentDirectory, ".tmpdir");
        Assert.EndsWith(".tmpdir", tempDirectory);
        return tempDirectory;
    }

    [Fact]
    public async Task ReadWrite_Success()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        string tempDirectory = CreateDbDir();
        Assert.EndsWith(".tmpdir", tempDirectory);
        try
        {
            using IArtifactRegistrationManager r = new DiskArtifactRegistrationManager(tempDirectory, isReadOnly: false);
            await TestReadWriteDatabase(r, testCancellationToken);
        }
        finally
        {
            Directory.Delete(tempDirectory, true);
        }
    }

    [Fact]
    public async Task ReadWrite_MultiAccess_Success()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        string tempDirectory = CreateDbDir();
        Assert.EndsWith(".tmpdir", tempDirectory);
        try
        {
            {
                using IArtifactRegistrationManager r = new DiskArtifactRegistrationManager(tempDirectory, isReadOnly: false);
                await TestReadWriteDatabase(r, testCancellationToken);
            }
            {
                using IArtifactRegistrationManager r = new DiskArtifactRegistrationManager(tempDirectory, isReadOnly: true);
                await VerifyWrittenDatabase(r, testCancellationToken);
            }
        }
        finally
        {
            Directory.Delete(tempDirectory, true);
        }
    }

    [Fact]
    public async Task ReadOnly_PreventsWrite()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        string tempDirectory = CreateDbDir();
        Assert.EndsWith(".tmpdir", tempDirectory);
        try
        {
            {
                using IArtifactRegistrationManager r = new DiskArtifactRegistrationManager(tempDirectory, isReadOnly: false);
                await TestReadWriteDatabase(r, testCancellationToken);
            }
            {
                using IArtifactRegistrationManager r = new DiskArtifactRegistrationManager(tempDirectory, isReadOnly: true);
                await TestDatabaseReadOnly(r, false, testCancellationToken);
            }
        }
        finally
        {
            Directory.Delete(tempDirectory, true);
        }
    }
}
