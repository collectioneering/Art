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

    /// <summary>
    /// Logs a message at <see cref="LogLevel.Information"/>.
    /// </summary>
    /// <param name="title">Log title.</param>
    /// <param name="body">Log body.</param>
    void LogInformation(string? title, string? body) => Log(title, body, LogLevel.Information);

    /// <summary>
    /// Logs a message at <see cref="LogLevel.Entry"/>.
    /// </summary>
    /// <param name="title">Log title.</param>
    /// <param name="body">Log body.</param>
    void LogEntry(string? title, string? body) => Log(title, body, LogLevel.Entry);

    /// <summary>
    /// Logs a message at <see cref="LogLevel.Title"/>.
    /// </summary>
    /// <param name="title">Log title.</param>
    /// <param name="body">Log body.</param>
    void LogTitle(string? title, string? body) => Log(title, body, LogLevel.Title);

    /// <summary>
    /// Logs a message at <see cref="LogLevel.Warning"/>.
    /// </summary>
    /// <param name="title">Log title.</param>
    /// <param name="body">Log body.</param>
    void LogWarning(string? title, string? body) => Log(title, body, LogLevel.Warning);

    /// <summary>
    /// Logs a message at <see cref="LogLevel.Error"/>.
    /// </summary>
    /// <param name="title">Log title.</param>
    /// <param name="body">Log body.</param>
    void LogEror(string? title, string? body) => Log(title, body, LogLevel.Error);
}
