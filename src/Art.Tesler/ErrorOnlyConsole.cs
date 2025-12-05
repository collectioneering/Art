namespace Art.Tesler;

/// <summary>
/// Provides access to a console that always writes to a single <see cref="TextWriter"/>.
/// </summary>
public class ErrorOnlyConsole : IConsole
{
    /// <summary>
    /// Initializes a new instance of <see cref="ErrorOnlyConsole"/>.
    /// </summary>
    public ErrorOnlyConsole(TextWriter textWriter)
    {
        Out = Error = textWriter;
    }

    /// <inheritdoc />
    public TextWriter Error { get; }

    /// <inheritdoc />
    public TextWriter Out { get; }
}
