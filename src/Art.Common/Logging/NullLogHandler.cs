namespace Art.Common.Logging;

/// <summary>
/// Log handler with no output.
/// </summary>
public class NullLogHandler : IToolLogHandler
{
    /// <inheritdoc />
    public LogPreferences LogPreferences
    {
        get => LogPreferences.Default;
        set { }
    }

    /// <summary>
    /// Shared instance.
    /// </summary>
    public static readonly NullLogHandler Instance = new();

    /// <inheritdoc/>
    public void Log(string tool, string group, string? title, string? body, LogLevel level)
    {
    }

    /// <inheritdoc/>
    public void Log(string? title, string? body, LogLevel level)
    {
    }
}
