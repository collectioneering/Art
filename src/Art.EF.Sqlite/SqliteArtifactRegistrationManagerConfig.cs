namespace Art.EF.Sqlite;

/// <summary>
/// Config for a <see cref="SqliteArtifactRegistrationManager"/>.
/// </summary>
/// <param name="ApplyMigrationsOnStartup">If true, allow migrations to execute automatically.</param>
/// <param name="IsReadOnly">If true, writes to the database are disabled.</param>
/// <param name="DisablePendingMigrationsCheck">If true, disables startup check for pending migrations (unsafe!).</param>
public record SqliteArtifactRegistrationManagerConfig(
    bool ApplyMigrationsOnStartup,
    bool IsReadOnly,
    bool DisablePendingMigrationsCheck
)
{
    public static readonly SqliteArtifactRegistrationManagerConfig Default = new(
        ApplyMigrationsOnStartup: true,
        IsReadOnly: false,
        DisablePendingMigrationsCheck: true
    );
}
