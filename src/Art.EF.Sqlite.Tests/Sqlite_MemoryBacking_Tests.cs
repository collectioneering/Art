using System.Threading.Tasks;
using Art.DbTestsBase;
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
        Assert.True(r.Context.SqliteUsingInMemory);
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
        Assert.True(r.Context.SqliteUsingInMemory);
        await TestReadWriteDatabase(r, testCancellationToken);
        await r.Context.Database.GetDbConnection().CloseAsync();
        await VerifyWrittenDatabase(r, testCancellationToken);
    }

    [Fact]
    public async Task MemoryAsFallback_ReadOnly_SucceedsWithInMemory()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        string memoryFileId = SqliteArtifactContextFactory.CreateMemoryFileId();
        {
            var factory = new SqliteArtifactContextFactory(requireInMemory: false, isReadOnly: false) { MemoryFileId = memoryFileId };
            var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
            await using var context = factory.CreateDbContext([]);
            await using SqliteArtifactRegistrationManager r = new(context, config);
            Assert.True(r.Context.SqliteUsingInMemory);
            await TestReadWriteDatabase(r, testCancellationToken);
        }
        {
            var factory = new SqliteArtifactContextFactory(requireInMemory: false, isReadOnly: true) { MemoryFileId = memoryFileId };
            var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: false, IsReadOnly: true, DisablePendingMigrationsCheck: false);
            await using var context = factory.CreateDbContext([]);
            await using SqliteArtifactRegistrationManager r = new(context, config);
            Assert.True(r.Context.SqliteUsingInMemory);
            await TestDatabaseReadOnly(r, testEmpty: true, testCancellationToken);
        }
    }

    [Fact]
    public async Task MemoryBacking_ReadOnly_Succeeds()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        string memoryFileId = SqliteArtifactContextFactory.CreateMemoryFileId();
        {
            var factory = new SqliteArtifactContextFactory(requireInMemory: true, isReadOnly: false) { MemoryFileId = memoryFileId };
            var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: true, IsReadOnly: false, DisablePendingMigrationsCheck: false);
            await using var context = factory.CreateDbContext([]);
            await using SqliteArtifactRegistrationManager r = new(context, config);
            Assert.True(r.Context.SqliteUsingInMemory);
            await TestReadWriteDatabase(r, testCancellationToken);
        }
        {
            var factory = new SqliteArtifactContextFactory(requireInMemory: true, isReadOnly: true) { MemoryFileId = memoryFileId };
            var config = new SqliteArtifactRegistrationManagerConfig(ApplyMigrationsOnStartup: false, IsReadOnly: true, DisablePendingMigrationsCheck: false);
            await using var context = factory.CreateDbContext([]);
            await using SqliteArtifactRegistrationManager r = new(context, config);
            Assert.True(r.Context.SqliteUsingInMemory);
            await TestDatabaseReadOnly(r, testEmpty: true, testCancellationToken);
        }
    }
}
