namespace Art.Common.Logging;

/// <summary>
/// Log handler output to two backing <see cref="TextWriter"/> (out and err) with no special formatting.
/// </summary>
public class PlainLogHandler : IToolLogHandler
{
    private readonly AutoResetEvent _wh;
    private readonly bool _itsumoError;
    private readonly TextWriter _out;
    private readonly TextWriter _warn;
    private readonly TextWriter _error;

    /// <inheritdoc />
    public LogPreferences LogPreferences { get; set; }

    /// <summary>
    /// Initializes an instance of <see cref="StyledLogHandler"/>.
    /// </summary>
    /// <param name="outWriter">Writer for normal output.</param>
    /// <param name="warnWriter">Writer for warning output.</param>
    /// <param name="errorWriter">Writer for error output.</param>
    /// <param name="logPreferences">Preferences to use when logging.</param>
    /// <param name="alwaysPrintToErrorStream">If true, always print output to error stream.</param>
    public PlainLogHandler(
        TextWriter outWriter,
        TextWriter warnWriter,
        TextWriter errorWriter,
        LogPreferences logPreferences,
        bool alwaysPrintToErrorStream)
    {
        _out = outWriter ?? throw new ArgumentNullException(nameof(outWriter));
        _warn = warnWriter ?? throw new ArgumentNullException(nameof(warnWriter));
        _error = errorWriter ?? throw new ArgumentNullException(nameof(errorWriter));
        LogPreferences = logPreferences;
        _wh = new AutoResetEvent(true);
        _itsumoError = alwaysPrintToErrorStream;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Logs level, tool, group, title, and body, in that order, using <see cref="TextWriter.WriteLine(String)"/>,
    /// skipping any of the listed that are null.
    /// </remarks>
    public void Log(string tool, string group, string? title, string? body, LogLevel level)
    {
        _wh.WaitOne();
        try
        {
            var textWriter = SelectTextWriter(level);
            textWriter.WriteLine(level.ToString());
            textWriter.WriteLine(tool);
            textWriter.WriteLine(group);
            if (title != null)
            {
                textWriter.WriteLine(title);
            }
            if (body != null)
            {
                textWriter.WriteLine(body);
            }
        }
        finally
        {
            _wh.Set();
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Logs level, title, and body, in that order, using <see cref="TextWriter.WriteLine(String)"/>,
    /// skipping any of the listed that are null.
    /// </remarks>
    public void Log(string? title, string? body, LogLevel level)
    {
        _wh.WaitOne();
        try
        {
            var textWriter = SelectTextWriter(level);
            textWriter.WriteLine(level.ToString());
            if (title != null)
            {
                textWriter.WriteLine(title);
            }
            if (body != null)
            {
                textWriter.WriteLine(body);
            }
        }
        finally
        {
            _wh.Set();
        }
    }

    private TextWriter SelectTextWriter(LogLevel level)
    {
        if (_itsumoError)
        {
            return _error;
        }
        return level switch
        {
            LogLevel.Information => _out,
            LogLevel.Entry => _out,
            LogLevel.Title => _out,
            LogLevel.Warning => _warn,
            LogLevel.Error => _error,
            _ => _error
        };
    }
}
