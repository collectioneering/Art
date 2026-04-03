using System.Reflection;
using Art.EF.Sqlite.Tests.Migrations1;
using Art.EF.Sqlite.Tests.Migrations2;
using Art.EF.Sqlite.Tests.TestSupport;
using Art.EF.TestsBase;

namespace Art.EF.Sqlite.Tests;

public class Sqlite_EFTests_Migrations : EFTests_Migrations
{
    protected override IEFTestDatabaseSource CreateDatabaseSource() => new SqliteTestDatabaseSource();

    protected override Assembly GetInitialCreateMigrationsAssembly() => typeof(TestSqliteMigrations1).Assembly;

    protected override Assembly GetDummyMigrationQX1MigrationsAssembly() => typeof(TestSqliteMigrations2).Assembly;
}
