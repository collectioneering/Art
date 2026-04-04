using System.Threading.Tasks;
using Art.DbTestsBase;
using Art.EF.TestsBase;
using Microsoft.EntityFrameworkCore;

namespace Art.EF.Sqlite.Tests;

public class Sqlite_MemoryBacking_Tests : ArtifactRegistrationManagerTestsBase
{
    [Fact]
    public async Task MemoryAsFallback_ReadWrite_SucceedsWithInMemory()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
        await using SqliteArtifactRegistrationManager r = new(false, config);
        Assert.True(r.Context.UsingInMemory);
        await TestReadWriteDatabase(r, testCancellationToken);
        await r.Context.Database.GetDbConnection().CloseAsync();
        await VerifyWrittenDatabase(r, testCancellationToken);
    }

    [Fact]
    public async Task MemoryBacking_ReadWrite_Succeeds()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
        await using SqliteArtifactRegistrationManager r = new(true, config);
        Assert.True(r.Context.UsingInMemory);
        await TestReadWriteDatabase(r, testCancellationToken);
        await r.Context.Database.GetDbConnection().CloseAsync();
        await VerifyWrittenDatabase(r, testCancellationToken);
    }

    [Fact]
    public async Task MemoryAsFallback_ReadOnly_SucceedsWithInMemory()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: false, IsReadOnly: true, DisablePendingMigrationsCheck: false);
        await using SqliteArtifactRegistrationManager r = new(false, config);
        Assert.True(r.Context.UsingInMemory);
        await TestDatabaseReadOnly(r, testEmpty: true, testCancellationToken);
    }

    [Fact]
    public async Task MemoryBacking_ReadOnly_Succeeds()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: false, IsReadOnly: true, DisablePendingMigrationsCheck: false);
        await using SqliteArtifactRegistrationManager r = new(true, config);
        Assert.True(r.Context.UsingInMemory);
        await TestDatabaseReadOnly(r, testEmpty: true, testCancellationToken);
    }
}
