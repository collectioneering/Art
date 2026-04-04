using System.Threading.Tasks;
using Art.DbTestsBase;

namespace Art.EF.TestsBase;

public abstract class EFTests_ReadWrite : ArtifactRegistrationManagerTestsBase
{
    protected abstract IEFTestDatabaseSource CreateDatabaseSource();

    [Fact]
    public async Task ReadWrite_Success()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        using var dbSource = CreateDatabaseSource();

        {
            var config = new TestDatabaseConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
            using IArtifactRegistrationManager r = dbSource.CreateArtifactRegistrationManager(config, null);
            await TestReadWriteDatabase(r, testCancellationToken);
        }
    }
}
