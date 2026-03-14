namespace Art;

/// <summary>
/// Represents general log handler.
/// </summary>
public interface ILogHandler
{
    /// <summary>
    /// Preferences to use when logging.
    /// </summary>
    LogPreferences LogPreferences { get; set; }

    /// <summary>
    /// Logs a message.
    /// </summary>
    /// <param name="title">Log title.</param>
    /// <param name="body">Log body.</param>
    /// <param name="level">Log level.</param>
    void Log(string? title, string? body, LogLevel level);
}
