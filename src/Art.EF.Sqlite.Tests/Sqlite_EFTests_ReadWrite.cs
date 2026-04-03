using Art.EF.Sqlite.Tests.TestSupport;
using Art.EF.TestsBase;

namespace Art.EF.Sqlite.Tests;

public class Sqlite_EFTests_ReadWrite : EFTests_ReadWrite
{
    protected override IEFTestDatabaseSource CreateDatabaseSource() => new SqliteTestDatabaseSource();
}
