using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Art.EF.TestsBase;

public abstract class EFTests_Migrations : EFTestsBase
{
    protected abstract IEFTestDatabaseSource CreateDatabaseSource();

    protected abstract Assembly GetInitialCreateMigrationsAssembly();

    protected abstract Assembly GetDummyMigrationQX1MigrationsAssembly();

    [Fact]
    public void ReadWrite_UnappliedInitialMigration_Fail()
    {
        using var dbSource = CreateDatabaseSource();

        {
            var config = new TestDatabaseConfig(ApplyMigrationsOnStartup: false, IsReadOnly: false, DisablePendingMigrationsCheck: false);
            var pendingMigrationsPresentException = Assert.Throws<EFPendingMigrationsPresentException>(() => dbSource.CreateArtifactRegistrationManager(config, GetInitialCreateMigrationsAssembly()));
            Assert.Single(pendingMigrationsPresentException.PendingMigrations);
            Assert.EndsWith("_InitialCreate", pendingMigrationsPresentException.PendingMigrations.First());
        }
    }

    [Fact]
    public void ReadOnly_UnappliedInitialMigration_Fail()
    {
        using var dbSource = CreateDatabaseSource();

        {
            var config = new TestDatabaseConfig(ApplyMigrationsOnStartup: false, IsReadOnly: true, DisablePendingMigrationsCheck: false);
            var pendingMigrationsPresentException = Assert.Throws<EFPendingMigrationsPresentException>(() => dbSource.CreateArtifactRegistrationManager(config, GetInitialCreateMigrationsAssembly()));
            Assert.Single(pendingMigrationsPresentException.PendingMigrations);
            Assert.EndsWith("_InitialCreate", pendingMigrationsPresentException.PendingMigrations.First());
        }
    }

    [Fact]
    public async Task ReadWrite_AppliedAllMigrations_Success()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        using var dbSource = CreateDatabaseSource();

        {
            var config = new TestDatabaseConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
            using IArtifactRegistrationManager r = dbSource.CreateArtifactRegistrationManager(config, GetInitialCreateMigrationsAssembly());
            await TestReadWriteDatabase(r, testCancellationToken);
        }

        {
            var config = new TestDatabaseConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
            using IArtifactRegistrationManager r = dbSource.CreateArtifactRegistrationManager(config, GetInitialCreateMigrationsAssembly());
            await VerifyWrittenDatabase(r, testCancellationToken);
        }
    }

    [Fact]
    public async Task ReadOnly_AppliedAllMigrations_Success()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        using var dbSource = CreateDatabaseSource();

        {
            var config = new TestDatabaseConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
            using IArtifactRegistrationManager r = dbSource.CreateArtifactRegistrationManager(config, GetInitialCreateMigrationsAssembly());
            await TestReadWriteDatabase(r, testCancellationToken);
        }

        {
            var config = new TestDatabaseConfig(ApplyMigrationsOnStartup: false, IsReadOnly: true, DisablePendingMigrationsCheck: false);
            using IArtifactRegistrationManager r = dbSource.CreateArtifactRegistrationManager(config, GetInitialCreateMigrationsAssembly());
            await VerifyWrittenDatabase(r, testCancellationToken);
        }
    }

    [Fact]
    public async Task ReadWrite_PendingNonInitialMigration_Fail()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        using var dbSource = CreateDatabaseSource();

        {
            var config = new TestDatabaseConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
            using IArtifactRegistrationManager r = dbSource.CreateArtifactRegistrationManager(config, GetInitialCreateMigrationsAssembly());
            await TestReadWriteDatabase(r, testCancellationToken);
        }

        {
            var config = new TestDatabaseConfig(ApplyMigrationsOnStartup: false, IsReadOnly: false, DisablePendingMigrationsCheck: false);
            var pendingMigrationsPresentException = Assert.Throws<EFPendingMigrationsPresentException>(() => dbSource.CreateArtifactRegistrationManager(config, GetDummyMigrationQX1MigrationsAssembly()));
            Assert.Single(pendingMigrationsPresentException.PendingMigrations);
            Assert.EndsWith("_DummyMigrationQX1", pendingMigrationsPresentException.PendingMigrations.First());
        }
    }

    [Fact]
    public async Task ReadOnly_PendingNonInitialMigration_Fail()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        using var dbSource = CreateDatabaseSource();

        {
            var config = new TestDatabaseConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
            using IArtifactRegistrationManager r = dbSource.CreateArtifactRegistrationManager(config, GetInitialCreateMigrationsAssembly());
            await TestReadWriteDatabase(r, testCancellationToken);
        }

        {
            var config = new TestDatabaseConfig(ApplyMigrationsOnStartup: false, IsReadOnly: true, DisablePendingMigrationsCheck: false);
            var pendingMigrationsPresentException = Assert.Throws<EFPendingMigrationsPresentException>(() => dbSource.CreateArtifactRegistrationManager(config, GetDummyMigrationQX1MigrationsAssembly()));
            Assert.Single(pendingMigrationsPresentException.PendingMigrations);
            Assert.EndsWith("_DummyMigrationQX1", pendingMigrationsPresentException.PendingMigrations.First());
        }
    }

    [Fact]
    public async Task ReadWrite_ApplyMigrationsInMultipleSteps_Success()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        using var dbSource = CreateDatabaseSource();

        {
            var config = new TestDatabaseConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
            using IArtifactRegistrationManager r = dbSource.CreateArtifactRegistrationManager(config, GetInitialCreateMigrationsAssembly());
            await TestReadWriteDatabase(r, testCancellationToken);
        }

        {
            var config = new TestDatabaseConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
            using IArtifactRegistrationManager r = dbSource.CreateArtifactRegistrationManager(config, GetDummyMigrationQX1MigrationsAssembly());
            await VerifyWrittenDatabase(r, testCancellationToken);
        }

        {
            var config = new TestDatabaseConfig(ApplyMigrationsOnStartup: false, IsReadOnly: false, DisablePendingMigrationsCheck: false);
            using IArtifactRegistrationManager r = dbSource.CreateArtifactRegistrationManager(config, GetDummyMigrationQX1MigrationsAssembly());
            await VerifyWrittenDatabase(r, testCancellationToken);
        }
    }

    [Fact]
    public async Task ReadOnly_ApplyMigrationsInMultipleSteps_Success()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        using var dbSource = CreateDatabaseSource();

        {
            var config = new TestDatabaseConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
            using IArtifactRegistrationManager r = dbSource.CreateArtifactRegistrationManager(config, GetInitialCreateMigrationsAssembly());
            await TestReadWriteDatabase(r, testCancellationToken);
        }

        {
            var config = new TestDatabaseConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
            using IArtifactRegistrationManager r = dbSource.CreateArtifactRegistrationManager(config, GetDummyMigrationQX1MigrationsAssembly());
            await VerifyWrittenDatabase(r, testCancellationToken);
        }

        {
            var config = new TestDatabaseConfig(ApplyMigrationsOnStartup: false, IsReadOnly: true, DisablePendingMigrationsCheck: false);
            using IArtifactRegistrationManager r = dbSource.CreateArtifactRegistrationManager(config, GetDummyMigrationQX1MigrationsAssembly());
            await VerifyWrittenDatabase(r, testCancellationToken);
        }
    }
}
