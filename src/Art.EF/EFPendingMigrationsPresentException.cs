namespace Art.EF;

/// <summary>
/// Provides details on pending migrations that have not been applied.
/// </summary>
public class EFPendingMigrationsPresentException : Exception
{
    /// <summary>
    /// List of pending migrations.
    /// </summary>
    public IReadOnlyList<string> PendingMigrations { get; }

    /// <summary>
    /// Initializes an instance of <see cref="EFPendingMigrationsPresentException"/>.
    /// </summary>
    /// <param name="pendingMigrations">List of pending migrations.</param>
    /// <param name="message">Exception message.</param>
    public EFPendingMigrationsPresentException(IReadOnlyList<string> pendingMigrations, string message) : base(message)
    {
        PendingMigrations = [..pendingMigrations];
    }
}
