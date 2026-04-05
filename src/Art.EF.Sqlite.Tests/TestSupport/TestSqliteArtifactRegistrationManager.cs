using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Art.EF.Sqlite.Tests.TestSupport;

public class TestSqliteArtifactRegistrationManager : SqliteArtifactRegistrationManager
{
    /// <summary>
    /// Creates a new instance of <see cref="TestSqliteArtifactRegistrationManager"/> with the specified Sqlite backing file.
    /// </summary>
    /// <param name="file">Sqlite backing file.</param>
    /// <param name="config">Configuration to use.</param>
    public TestSqliteArtifactRegistrationManager(
        string file,
        SqliteArtifactRegistrationManagerConfig config
    )
        : base(file, config)
    {
    }

    public TestSqliteArtifactRegistrationManager(
        SqliteArtifactContext context,
        SqliteArtifactRegistrationManagerConfig config
    )
        : base(context, config)
    {
    }

    /// <summary>
    /// Clears Sqlite pool if applicable.
    /// </summary>
    protected internal static void ClearPoolForSqliteConnection(SqliteArtifactContext context)
    {
        if (context is { Database: var databaseFacade }
            && databaseFacade.GetDbConnection() is SqliteConnection sqliteConnection)
        {
            SqliteConnection.ClearPool(sqliteConnection);
        }
    }

    protected override void Dispose(bool disposing)
    {
        try
        {
            if (disposing)
            {
                ClearPoolForSqliteConnection(Context);
            }
        }
        finally
        {
            base.Dispose(disposing);
        }
    }
}
