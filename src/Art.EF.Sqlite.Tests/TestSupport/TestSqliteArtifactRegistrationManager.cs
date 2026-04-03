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
        SqliteArtifactContextFactory factory,
        SqliteArtifactRegistrationManagerConfig config
    )
        : base(factory, config)
    {
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ClearPoolForSqliteConnection(Context);
        }
        base.Dispose(disposing);
    }
}
