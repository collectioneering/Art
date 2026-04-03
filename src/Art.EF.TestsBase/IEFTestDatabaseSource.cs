using System;
using System.Reflection;

namespace Art.EF.TestsBase;

public interface IEFTestDatabaseSource : IDisposable
{
    public IArtifactRegistrationManager CreateArtifactRegistrationManager(TestDatabaseConfig config, Assembly? migrationsAssembly);
}

/// <summary>
/// Config for an <see cref="IArtifactRegistrationManager"/> from an <see cref="IEFTestDatabaseSource"/>.
/// </summary>
/// <param name="ApplyMigrationsOnStartup">If true, allow migrations to execute automatically.</param>
/// <param name="IsReadOnly">If true, writes to the database are disabled.</param>
/// <param name="DisablePendingMigrationsCheck">If true, disables startup check for pending migrations (unsafe!).</param>
public record TestDatabaseConfig(
    bool ApplyMigrationsOnStartup,
    bool IsReadOnly,
    bool DisablePendingMigrationsCheck
);
